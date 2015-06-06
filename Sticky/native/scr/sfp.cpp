#pragma warning( disable: 4127 4800 4702 )
#include <string>
#include <list>
#include <vector>
#include <complex>
#include <windows.h>
#include <commctrl.h>
#include <regstr.h>
#include <stdlib.h>
#include <math.h>
#include <tchar.h>
typedef std::basic_string<TCHAR> tstring;
using namespace std;
#include "../body.h"
#include "../utils.h"
#include "../unzip.h"
using namespace stk;





int testparade=-1;       // if >=0, then we do a boring inspection-parade, for debugging
const tstring DebugFile= _T(""); //_T("OutputDebugString");
const bool SCRDEBUG = (DebugFile!=_T(""));
//
// These global variables are loaded at the start of WinMain
BOOL  MuteSound;
DWORD MouseThreshold;  // In pixels
DWORD PasswordDelay;   // In seconds. Doesn't apply to NT/XP/Win2k.
// also these, which are used by the General properties dialog
DWORD PasswordDelayIndex;  // 0=seconds, 1=minutes. Purely a visual thing.
DWORD MouseThresholdIndex; // 0=high, 1=normal, 2=low, 3=ignore. Purely visual
TCHAR Corners[5];          // "-YN-" or something similar
BOOL  HotServices;         // whether they're present or not
// and these are created when the dialog/saver starts
POINT InitCursorPos;
DWORD InitTime;        // in ms
bool  IsDialogActive;
bool  ReallyClose;     // for NT, so we know if a WM_CLOSE came from us or it.
// Some other minor global variables and prototypes
HINSTANCE hInstance=0;
HICON hiconsm=0,hiconbg=0;
HBITMAP hbmmonitor=0;  // bitmap for the monitor class
tstring SaverName;     // this is retrieved at the start of WinMain from String Resource 1
vector<RECT> monitors; // the rectangles of each monitor (smSaver) or of the preview window (smPreview)
struct TSaverWindow;
const UINT SCRM_GETMONITORAREA=WM_APP; // gets the client rectangle. 0=all, 1=topleft, 2=topright, 3=botright, 4=botleft corner
inline void Debug(const tstring s);
void SetDlgItemUrl(HWND hdlg,int id,const TCHAR *url);
void   RegSave(const tstring name,int val);
void   RegSave(const tstring name,bool val);
void   RegSave(const tstring name,tstring val);
int    RegLoad(const tstring name,int def);
bool   RegLoad(const tstring name,bool def);
tstring RegLoad(const tstring name,tstring def);

//
// IMPORTANT GLOBAL VARIABLES:
enum TScrMode {smNone,smConfig,smPassword,smPreview,smSaver,smInstall,smUninstall} ScrMode=smNone;
vector<TSaverWindow*> SaverWindow;   // the saver windows, one per monitor. In preview mode there's just one.

// -----------------------------------------------------------------------------
// -----------------------------------------------------------------------------



struct TBodyInfo
{ string path; string desc; stk::TBody *b; bool failed;
  TBodyInfo() : path(""), desc(""), b(0), failed(false) {}
};

// These are the saver's global settings
int refcount=0;
bool TryAudio=true;      // do we try to listen to the audio?
tstring banner; unsigned int bantime=0;  // a debug banner
vector<TBodyInfo> gbods; // all the sticks we've found
int firststick=-1;       // start with "justin"
const double LineThickness=0.03;
const double tlength=14; // seconds for a body to cross the screen
const double tzoom=9;    // seconds for a body to go from already-here to fully-zoomed (around 11/16 of tlength)
//
// Configuration of the audio-source. Windows supports 'unsigned char' and (signed) 'short'
typedef unsigned char sample_t;
const int waveid=WAVE_MAPPER, nchannels=2, frequency=22050;
const int nbufs=8, buf_nsamples=1024;  // nsamples must be a power of 2, at least 1024
const int buf_nlog=10, buf_nsqrt=32;   // log and sqrt ie. 2^nlog==nsqrt^2==nsamples
//
// Internal stuff for audio
sample_t wavbuf[nbufs*buf_nsamples*nchannels];  // all the buffers go one after the other. They store left/right alternating
WAVEHDR wavheader[nbufs];    // this is a pool of headers
// The following three resare initialized by AudioStart(), and finished by AudioStop()
int wavcurbuf=-1;         // If >=0, it signifies that all headers are prepared, and the system is currently writing into 'wavcurbuf'
HWND hControlWnd=0;       // If !=0, then the callback window has been created
HWAVEIN hwavein=0;        // If !=0, then the audio system has been opened and started
bool fft_inited=false;    // gets set to true when the fft precomputed tables have been done
complex<double> fft_W[buf_nlog+1][buf_nsamples];  // precomputed complex exponentials
int fft_bitrev[buf_nsamples];                      // precomputed bit-reversal table
//
// Data that we receive from the audio
list<HWND> hnotify;                  // when a new sample has been done, send a WM_APP message to these
sample_t *waveformData=wavbuf;       // use waveformData[i*nchannels+chan]
unsigned char spectrumData[buf_nsamples][nchannels]; // this is the result of an FFT. Max frequency is frequency/2
double wmin[6],wmax[6],kmin[6],kmax[6];// for normalising it
double freq[3][6];                   // normalised fft


//
// This is a compile-time strongly-typed way of getting the attributes of the sample type
// In the olden days we'd have done it with #defines. Boo! Hiss! Down with defines!
template<class T> struct srange {};
template<> struct srange<unsigned char>
{ static unsigned char smin() {return 0;}
  static unsigned char smax() {return UCHAR_MAX;}
  static unsigned char smid() {return UCHAR_MAX/2;}
  static signed char s2schr(unsigned char i) {return (signed char)(((int)i)-128);}
};
template<> struct srange<short>
{ static short smin() {return SHRT_MIN;}
  static short smax() {return SHRT_MAX;}
  static short smid() {return 0;}
  static signed char s2schr(short i) {return (signed char)(i/256);}
};

void stk::luassertmsg(const char *s,const char *msg,const char *f,unsigned int l)
{ char c[2000]; wsprintf(c,"%s '%s' - %s:%u",msg,s,f,l);
  banner=c; bantime=GetTickCount()+5000;
}

int _matherr(struct _exception *e)
{ char c[200]; const char *err="ERR";
  if (e->type==DOMAIN) err="DOMAIN";
  else if (e->type==SING) err="SING";
  else if (e->type==OVERFLOW) err="OVERFLOW";
  else if (e->type==UNDERFLOW) err="UNDERFLOW";
  else if (e->type==PLOSS) err="PLOSS";
  else if (e->type==TLOSS) err="TLOSS";
  sprintf(c,"Matherr %s - %s(%g,%g)",err,e->name,e->arg1,e->arg2);
  banner=c; bantime=GetTickCount()+5000;
  e->arg1=0.01; e->arg2=0.01; e->retval=0.01;
  return 1;
}


void CommonInit()
{ refcount++; if (refcount!=1) return;
  srand(GetTickCount());
  //
  TryAudio=RegLoad(_T("TryAudio"),true);
  //
  vector<stk::TPre> pre;
  DirScan(pre,GetStickDir()+"\\");
  if (pre.size()==0)
  { for (int i=1; ; i++)
    { TCHAR c[MAX_PATH]; int res=LoadString(NULL,i+1000,c,MAX_PATH); if (res==0) break;
      TPre p; p.desc=c; wsprintf(c,"*%i",i+1000); p.path=c;
      pre.push_back(p);
    }
  }
  gbods.resize(pre.size());
  firststick=-1;
  for (int i=0; i<(int)pre.size(); i++)
  { gbods[i].path=pre[i].path;
    gbods[i].desc=pre[i].desc;
    gbods[i].b=0;
    gbods[i].failed=false;
    if (stk::StringLower(pre[i].desc).find("justin")!=string::npos) firststick=i;
  }
}

void CommonExit()
{ refcount--; if (refcount!=0) return;
  for (vector<TBodyInfo>::iterator i=gbods.begin(); i!=gbods.end(); i++) {if (i->b!=0) delete i->b; i->b=0;}
  gbods.clear();
}


struct OldestRecord
{ int index; unsigned int time;
  OldestRecord(int _index,unsigned int _time) : index(_index), time(_time) {}
  bool operator<(const OldestRecord &b) const {return time>b.time;}
};






// Fast Fourier Transform - assume that the tables bitrev[] and W[] have been precomputed.
// We take the source data in waveformData[], and write its fourier into spectrumData[]
// We also put normalised stuff into freq[][]
//
void fft()
{ if (!fft_inited)
  { for (int lg=1, twos=2; lg<=buf_nlog; lg++, twos*=2)
    { for (int i=0; i<buf_nsamples; i++ ) fft_W[lg][i] = complex<double>(cos(2.0*pi*i/twos),-sin(2.0*pi*i/twos));
    }
    for (int rev=0, i=0; i<buf_nsamples-1; i++)
    { fft_bitrev[i] = rev;
      int mask = buf_nsamples/2;  
      while (rev >= mask) {rev-=mask; mask >>= 1;}
      rev += mask;
    }
    fft_bitrev[buf_nsamples-1] = buf_nsamples-1;
    fft_inited=true;
  }
  //
  complex<double> X[nchannels][buf_nsamples];
  for (int chan=0; chan<nchannels; chan++)
  { for (int i=0; i<buf_nsamples; i++)
    { signed char sc = srange<sample_t>::s2schr(waveformData[i*nchannels+chan]);
      X[chan][fft_bitrev[i]]=complex<double>(sc,0.0);
    }
  }
  for (int chan=0; chan<nchannels; chan++)
  { int step = 1;
    for (int level=1; level<=buf_nlog; level++)
    { int increm = step*2;
      for (int j=0; j<step; j++)
      { complex<double> U = fft_W[level][j];   // U=exp(-2PIj / 2^level)
        for (int i=j; i<buf_nsamples; i+=increm)
        { complex<double> T = U;
          T *= X [chan][i+step];
          X [chan][i+step] = X[chan][i];
          X [chan][i+step] -= T;
          X [chan][i] += T;
        }
      }
      step *= 2;
    }
  }
  for (int chan=0; chan<nchannels; chan++)
  { for (int i=0; i<buf_nsamples; i++)
    { double re=X[chan][i].real(), im=X[chan][i].imag();
      double d = sqrt(re*re+im*im);
      spectrumData[i][chan] = (unsigned char)(d/buf_nsqrt);
    }
  }
  // now we normalize into six buckets.
  const double fftfreq = ((double)frequency)/2;        // the maximum frequency of the fft
  const double maxfreq = (fftfreq<=2515)?fftfreq:2515; // the max frequency we'll inspect
  const double minfreq = 450;
  const int boff = (int)(((double)buf_nsamples)*minfreq/fftfreq); // index of minfreq 
  const int bmax = (int)(((double)buf_nsamples)*maxfreq/fftfreq); // index of maxfreq
  const int bwidth = (bmax-boff)/6;                               // number of samples for each bucket
  //
  for (int p=0; p<6; p++)
  { freq[0][p]=0;
    freq[1][p]=0;
    for (int i=0; i<bwidth; i++)
    { int b = boff+p*bwidth+i;
      freq[0][p] += spectrumData[b][0];
      freq[1][p] += spectrumData[b][1];
    }
    if (freq[0][p]!=0) freq[0][p] = sqrt(freq[0][p])*100;
    if (freq[1][p]!=0) freq[1][p] = sqrt(freq[1][p])*100;
    //
    if (freq[0][p]>wmax[p]) wmax[p]=freq[0][p];
    if (freq[1][p]>wmax[p]) wmax[p]=freq[1][p];
    if (freq[0][p]<wmin[p] || freq[1][p]<wmin[p]) wmin[p]*=0.9;
  }
  // and get the karaoke
  int kvoc=0, kmus=0;
  for (int i=0; i<buf_nsamples; i++)
  { int left=srange<sample_t>::s2schr(waveformData[i*nchannels]), right=srange<sample_t>::s2schr(waveformData[i*nchannels+1]);
    int voc = (left+right)/2;
    int mus = (left-right);
    kvoc += voc*voc; kmus += mus*mus;
  }
  freq[2][0]=(int)sqrt(kvoc); freq[2][1]=(int)sqrt(kmus);
  if (freq[2][0]<kmin[0]) kmin[0]=freq[2][0];  if (freq[2][0]>kmax[0]) kmax[0]=freq[2][0];
  if (freq[2][1]<kmin[1]) kmin[1]=freq[2][1];  if (freq[2][1]>kmax[1]) kmax[1]=freq[2][1];
  // 
  // Now we normalise them into the range 0..1
  for (int p=0; p<6; p++)
  { freq[0][p] = (freq[0][p]-wmin[p]) / (wmax[p]-wmin[p]+1); if (freq[0][p]<0) freq[0][p]=0; freq[0][p]*=freq[0][p];
    freq[1][p] = (freq[1][p]-wmin[p]) / (wmax[p]-wmin[p]+1); if (freq[1][p]<0) freq[1][p]=0; freq[1][p]*=freq[1][p];
  }
  for (int p=0; p<2; p++)
  { freq[2][p] = (freq[2][p]-kmin[p]) / (kmax[p]-kmin[p]+1); if (freq[2][p]<0) freq[2][p]=0; freq[2][p]*=freq[2][p];
  }
}


LRESULT CALLBACK ControlWndProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ if (msg!=MM_WIM_DATA) return DefWindowProc(hwnd,msg,wParam,lParam);
  //
  // this is called when the currently-working-on buffer has finished. So we recycle it.
  if ((wavheader[wavcurbuf].dwFlags & WHDR_DONE)==0) return 0; // just to be sure
  //
  // Set up waveformData/spectrumData
  MSG mmsg; if (!fft_inited || !PeekMessage(&mmsg,0,0,0,PM_NOREMOVE))
  { waveformData=&wavbuf[wavcurbuf*buf_nsamples*nchannels]; fft();
    // and tell our audience!
    for (list<HWND>::const_iterator i=hnotify.begin(); i!=hnotify.end(); i++) SendMessage(*i,WM_APP,0,0);
  }
  //
  //
  waveInUnprepareHeader(hwavein,&wavheader[wavcurbuf],sizeof(WAVEHDR));
  wavheader[wavcurbuf].lpData=(LPSTR)&wavbuf[wavcurbuf*buf_nsamples*nchannels];
  wavheader[wavcurbuf].dwBufferLength=sizeof(sample_t)*nchannels*buf_nsamples;
  wavheader[wavcurbuf].dwFlags=0;
  wavheader[wavcurbuf].dwLoops=0;
  waveInPrepareHeader(hwavein,&wavheader[wavcurbuf],sizeof(WAVEHDR));
  waveInAddBuffer(hwavein,&wavheader[wavcurbuf],sizeof(WAVEHDR));
  //
  wavcurbuf++; if (wavcurbuf==nbufs) wavcurbuf=0;
  //
  return 0;
}





void AudioStart(HWND hwnd)
{ // Initialize the audio. Is safe if several windows ask for notification
  //
  hnotify.push_back(hwnd);
  //
  if (hControlWnd==0)
  { WNDCLASS wc; ZeroMemory(&wc,sizeof(wc)); 
    wc.hInstance=hInstance;
    wc.lpszClassName=_T("ControlWnd");
    wc.lpfnWndProc=ControlWndProc;
    RegisterClass(&wc);
    hControlWnd = CreateWindow(_T("ControlWnd"),_T(""),0,0,0,0,0,NULL,0,hInstance,0);
  }
  //
  if (hwavein==0)
  { WAVEFORMATEX format; ZeroMemory(&format,sizeof(format));
    format.wFormatTag=WAVE_FORMAT_PCM;
    format.nChannels=nchannels;
    format.nSamplesPerSec=frequency;
    format.wBitsPerSample=sizeof(sample_t)*8;
    format.nBlockAlign=format.wBitsPerSample*nchannels/8;
    format.nAvgBytesPerSec=frequency*format.nBlockAlign;
    MMRESULT mres=waveInOpen(0,waveid,&format,0,0,WAVE_FORMAT_QUERY);
    if (mres==MMSYSERR_NOERROR) mres=waveInOpen(&hwavein,waveid,&format,(DWORD)hControlWnd,0,CALLBACK_WINDOW);
    if (mres!=MMSYSERR_NOERROR)
    { TCHAR c[1024]; MMRESULT r2=waveInGetErrorText(mres,c,1024);
      if (r2==MMSYSERR_BADERRNUM) Debug(_T("MMERR_???"));
      else if (r2==MMSYSERR_NODRIVER) Debug(_T("No driver present"));
      else if (r2==MMSYSERR_NOMEM) Debug(_T("Unable to allocate or lock memory"));
      else Debug(c);
      return;
    }
  }
  //
  if (wavcurbuf==-1)
  { for (int i=0; i<nbufs; i++)
    { wavheader[i].lpData=(LPSTR)&wavbuf[i*buf_nsamples*nchannels];
      wavheader[i].dwBufferLength=sizeof(sample_t)*nchannels*buf_nsamples;
      wavheader[i].dwFlags=0;
      wavheader[i].dwLoops=0;
      waveInPrepareHeader(hwavein,&wavheader[i],sizeof(WAVEHDR));
      waveInAddBuffer(hwavein,&wavheader[i],sizeof(WAVEHDR));
    }
    wavcurbuf=0;
  }
  freq[0][0]=0; freq[1][0]=0; wmin[0]=150; wmax[0]=2600;
  freq[0][1]=0; freq[1][1]=0; wmin[0]=140; wmax[0]=2300;
  freq[0][2]=0; freq[1][2]=0; wmin[0]=130; wmax[0]=2100;
  freq[0][3]=0; freq[1][3]=0; wmin[0]=120; wmax[0]=1900;
  freq[0][4]=0; freq[1][4]=0; wmin[0]=110; wmax[0]=1700;
  freq[0][5]=0; freq[1][5]=0; wmin[0]=100; wmax[0]=1500;
  freq[2][0]=0; kmin[0]=20; kmax[0]=400;
  freq[2][1]=0; kmin[1]=20; kmax[1]=400;
  //
  waveInStart(hwavein);
}

void AudioStop(HWND hwnd)
{ // This says that a window no longer wants notification. If no more windows want it, then we stop.
  for (list<HWND>::iterator i=hnotify.begin(); i!=hnotify.end(); i++)
  { if (hwnd==*i) {hnotify.erase(i); break;}
  }
  if (hnotify.size()>0) return;
  //
  if (hwavein!=0) waveInReset(hwavein);
  if (wavcurbuf!=-1)
  { for (int i=0; i<nbufs; i++) waveInUnprepareHeader(hwavein,&wavheader[i],sizeof(WAVEHDR));
    wavcurbuf=-1;
  }
  if (hwavein!=0) waveInClose(hwavein); hwavein=0;
  if (hControlWnd!=0) DestroyWindow(hControlWnd); hControlWnd=0;
}





bool OpenFile(stk::TBody **b,string fn,char *err)
{ if (fn=="") return false;
  if (fn[0]!='*') return LoadBody(b,fn.c_str(),err,lbForUse);
  int id=atoi(fn.substr(1).c_str());
  HRSRC hrsrc = FindResource(hInstance,MAKEINTRESOURCE(id),RT_RCDATA);
  DWORD size = SizeofResource(hInstance,hrsrc);
  HGLOBAL hglob = LoadResource(hInstance,hrsrc);
  char *rs = (char*)LockResource(hglob);
  vector<char> buf(size+1);
  CopyMemory(&buf[0],rs,size); buf[size]=0;
  bool res=(*b)->ReadData(&buf[0],err,rdOverwrite,NULL);
  return res;
}





void MonitorInit();
void MonitorTick();


struct TSaverWindow
{ HWND hwnd; int id; // id=-1 for a preview, or 0..n for full-screen on the specified monitor
  int cw,ch;         // width, height (both of window and of backbuffer)
  HBITMAP hbmBuffer; // the backbuffer
  double tw;         // twelth of a unit - our basic scale for the screen
  int y1,y2;         // top and bottom of the procession
  bool timer;        // if we're using a timer, this is it
  unsigned int tsound; // previews/primary monitors keep track of sound, and of when last we heard it

  //
  TSaverWindow(HWND _hwnd,int _id) : hwnd(_hwnd),id(_id), hbmBuffer(0), timer(false), tsound(GetTickCount()-2000)
  { CommonInit(); if (id<=0) MonitorInit(); if (id<=0 && TryAudio) AudioStart(hwnd);
    if (id<=0 && !TryAudio) {timer=true; SetTimer(hwnd,1,100,NULL);}
    RECT rc; GetClientRect(hwnd,&rc); cw=rc.right; ch=rc.bottom;
    HDC sdc=GetDC(0); hbmBuffer=CreateCompatibleBitmap(sdc,cw,ch); ReleaseDC(0,sdc);
    tw=((double)cw)/16.0; // tw=twelth of a unit
    y1=ch*3/8; y2=ch*5/8;
  }
  ~TSaverWindow()
  { AudioStop(hwnd);
    if (hbmBuffer!=0) DeleteObject(hbmBuffer); hbmBuffer=0;
    CommonExit();
    if (timer) KillTimer(hwnd,1); timer=false;
  }
  void OnTimer()
  { for (int chan=0; chan<2; chan++)
    { for (int band=0; band<6; band++)
      { double d=(double)(rand()%1000)/1000;
        freq[chan][band]=d*d;
      }
    }
    freq[2][0]=freq[0][3]; freq[2][1]=freq[1][4];
    MonitorTick();
  }
  void OnPaint(HDC hdc,const RECT &)
  { HDC bufdc=CreateCompatibleDC(hdc); SelectObject(bufdc,hbmBuffer);
    BitBlt(hdc,0,0,cw,ch,bufdc,0,0,SRCCOPY);
    DeleteDC(bufdc);
  }
  void OtherWndProc(UINT msg,WPARAM,LPARAM)
  { if (msg!=WM_APP) return;
    unsigned int nowt=GetTickCount();  bool anysound=false;
    for (int p=0; p<6 && !anysound; p++) anysound |= (freq[0][p]>0.05 || freq[1][p]>0.05);
    if (!anysound && nowt>tsound+3000) {OnTimer(); return;}
    if (anysound) tsound=nowt;
    MonitorTick();
  }
  void Update();
};




vector<int> inuse; list<int> finisheds; int ninuse=0;

void PushActive(int gindex)
{ if (inuse.size()==0) {inuse.resize(gbods.size()); ninuse=0;}
  inuse[gindex]++; ninuse++;
}
void PopActive(int gindex)
{ if (inuse[gindex]==0) {Debug("*ERR* tried to pop nonexistent "+gbods[gindex].desc); return;}
  inuse[gindex]--; ninuse--;
  finisheds.push_back(gindex);
  while (finisheds.size()>10)
  { int i=finisheds.front(); finisheds.pop_front();
    if (inuse[i]==0 && gbods[i].b!=0) {delete gbods[i].b; gbods[i].b=0;}
  }
}





// The procession is represented by a queue of processums. The
// rightmost item (front) is the one that controls what we're doing (i.e.
// how we should move along). Each one can draw itself and indicate
// how much space it needs. We also record, for the rightmost one,
// how far in it is.

enum TProcessState {psHead, psNormal, psZoomin, psZoomed, psZoomout};

class TProcessData
{
public:
  int gindex; string d;
  TProcessState state;
  double noff;  // offset, for the psHead or Zoom* at the head of the list, as a multiple of tw
  double fzoom; // fraction zoomed, for Zoomin and Zoomout
  unsigned int zoomstart; // time when we started the zoomin/zoomout
  double tzoom; // time left, in seconds, for Zoomed.
  TProcessData() : state(psHead), noff(0), fzoom(0), gindex(-1)
  { if (gbods.size()==0) return;
    while (gindex==-1 || gbods[gindex].b==0)
    { if (testparade!=-1) {gindex=testparade; testparade=(testparade+1)%gbods.size();}
      else if (firststick!=-1 && firststick<(int)gbods.size()) {gindex=firststick; firststick=-1;}
      else gindex=rand()%gbods.size();
      if (gbods[gindex].failed) continue;
      //
      d=gbods[gindex].desc;
      if (gbods[gindex].b==0)
      { TBody *b = new stk::TBody(); char err[1000];
        bool res = OpenFile(&b,gbods[gindex].path,err);
        if (res) gbods[gindex].b=b;
        else {delete b; gbods[gindex].failed=true;}
      }
    }
  };
};







// MonitorInit, MonitorTick -- we support multiple monitors. Therefore
// we keep this global data for the procession as a whole, not tied to
// any particular monitor.

typedef list<TProcessData> TProcession;
vector<TProcession> processions; // Each procession goes from right to left, with the head at the right
int nprocs,nprim;       // how many processions there are, and which is the primary
vector<int> pmons;      // from left to right, a list of the monitors for each procession
unsigned int numtozoom; // how many sticks must pass before we do the next zoom
unsigned int tick;      // time of the previous calc+draw
TProcessData zoomgap; bool gotgap=false; // when zooming in, we devour one more figure to the right of the zoom


struct XSortedRect
{ int m, left,top,right,bottom;
  XSortedRect(int _m, int _left,int _top,int _right,int _bottom) : m(_m), left(_left), top(_top), right(_right), bottom(_bottom) {}
  bool operator<(const XSortedRect &b) const {return left<b.left;}
};

void MonitorInit()
{ // we parse the monitors[] array, to figure out a left-to-right sequence
  // of monitors to use for the procession
  // ie. start with primary monitor, then look for the closest one to left
  // or right whose left/right isn't overlapping, and so on
  list<XSortedRect> smons;
  int m=0; for (vector<RECT>::const_iterator i=monitors.begin(); i!=monitors.end(); i++,m++)
  { smons.push_back(XSortedRect(m,i->left,i->top,i->right,i->bottom));
  }
  smons.sort();
  //
  pmons.push_back(0); int x=monitors[0].right;
  for (list<XSortedRect>::const_iterator i=smons.begin(); i!=smons.end(); i++)
  { if (i->left>=x) {pmons.push_back(i->m); x=i->right;}
  }
  x=monitors[0].left; nprim=0;
  for (list<XSortedRect>::reverse_iterator i=smons.rbegin(); i!=smons.rend(); i++)
  { if (i->right<=x) {pmons.insert(pmons.begin(),i->m); x=i->left; nprim++;}
  }
  //
  nprocs=pmons.size();
  processions.resize(nprocs);
}



void PushItem(const TProcessData d, int m)
{ TProcession &procession = processions[m];
  double noff=0;
  if (procession.size()>0)
  { if (procession.front().state!=psZoomout) procession.front().state=psNormal;
    noff = procession.front().noff-6;
  }
  else if (m==nprim) numtozoom=10+rand()%10; // numtozoom only measures the primary monitor
  //
  procession.push_front(d);
  procession.front().noff=noff;
  procession.front().state=psHead;
  PushActive(d.gindex);
  //
  if (m==nprim) numtozoom--;  
  if (numtozoom==0 && m==nprim && testparade==-1)
  { procession.front().state=psZoomin;
    procession.front().zoomstart=tick;
    numtozoom = 15+rand()%15;
  }
}



void MonitorTick()
{ 
  if (processions[nprocs-1].size()==0)
  { PushItem(TProcessData(),nprocs-1);
    tick=GetTickCount();
  }
  // process a little
  unsigned int t = GetTickCount(); double cmul=((double)(t-tick))/10.0, dt=cmul/100.0; tick=t;
  double speedup=1; if (testparade!=-1) speedup=3;
  dt *= speedup;
  //
  bool zoomin = (processions[nprim].size()>0 && processions[nprim].front().state==psZoomin);
  bool zoomed = (processions[nprim].size()>0 && processions[nprim].front().state==psZoomed);
  bool zoomout=false;
  for (list<TProcessData>::const_iterator i=processions[nprim].begin(); i!=processions[nprim].end(); i++)
  { if (i->state==psZoomout) {zoomout=true; break;}
  }

  if (zoomin)
  { TProcession &procession = processions[nprim];
    procession.front().fzoom = ((double)(tick-procession.front().zoomstart))/1000.0/(tzoom/speedup);
    if (procession.front().fzoom>=1)
    { procession.front().fzoom=1;
      procession.front().state=psZoomed;
      procession.front().tzoom=(30+rand()%30)/speedup;
    }
  }

  if (zoomed)
  { TProcession &procession = processions[nprim];
    TProcessData &d = procession.front();
    d.tzoom -= dt;
    if (d.tzoom<=0)
    { d.state=psZoomout;
      d.zoomstart=tick;
      if (gotgap) {zoomgap.state=psHead; PushItem(zoomgap,nprim); PopActive(zoomgap.gindex);}
      else PushItem(TProcessData(),nprim);
      procession.front().noff=0;
    }
  }

  for (int m=0; m<nprocs; m++)
  { TProcession &procession = processions[m];
    if (zoomin && m>=nprim && gotgap) continue;
    if (zoomed && (m<=nprim || gotgap)) continue;
    if (zoomout && m<nprim) continue;
    // invariant: procession.front().state==psHead
    TProcessData &d = procession.front();
    d.noff += 16*dt/tlength;
    if (m==nprim && (zoomin || zoomed))
    { d.noff = min(d.noff,0.0);
    }
    if (d.noff>6 && m==nprocs-1)  PushItem(TProcessData(),m);
    // we only push new elements on the rightmost monitor
  }

  for (list<TProcessData>::iterator i=processions[nprim].begin(); i!=processions[nprim].end(); i++)
  { if (i->state==psZoomout)
    { i->fzoom = 1.0 - ((double)(tick-i->zoomstart))/1000.0/(tzoom/speedup);
      if (i->fzoom<=0) {i->state=psNormal; gotgap=false;}
    }
  }

  // draw it
  for (int m=0; m<nprocs; m++)
  { TSaverWindow &sav = *SaverWindow[pmons[m]];
    TProcession &procession = processions[m];
    RECT rc; rc.top=0; rc.left=0; rc.right=sav.cw; rc.bottom=sav.ch;
    double tw=sav.tw;
    HDC hdc=GetDC(sav.hwnd), bufdc=CreateCompatibleDC(hdc);
    SelectObject(bufdc,sav.hbmBuffer);
    FillRect(bufdc,&rc,(HBRUSH)GetStockObject(BLACK_BRUSH));
    //
    int x=sav.cw-(int)(procession.front().noff*tw);
    bool lastvisible=false;
    for (list<TProcessData>::const_iterator i=procession.begin(); i!=procession.end(); i++)
    { stk::TBody *b=gbods[i->gindex].b; 
      if (b==0) {Debug("*ERR* missing body "+gbods[i->gindex].desc); continue;}
      b->AssignFreq(freq,cmul);
      b->RecalcEffects();
      b->Recalc();
      // draw it.
      RECT brc;
      if (i->state==psHead || i->state==psNormal)
      { brc.left=x; brc.top=sav.y1; brc.right=(int)(x+tw*3); brc.bottom=(int)(sav.y1+tw*3);//sav.y2;
        string err;  bool ok=SimpleDraw(bufdc,brc,b,0,0,&err);
        if (!ok) {banner=err; bantime=tick+5000;}
        lastvisible = (brc.right>0);
        x -= (int)(sav.tw*6.0);
      }
      else if (i->state==psZoomin || i->state==psZoomed || i->state==psZoomout)
      { double f = i->fzoom;// * i->fzoom;
        double margright;
        if (i->state==psZoomout) margright = -tw*6.0 + f*tw*2.0 + (1-f)*tw*3.0;
        else margright = f*tw*2.0 - (1-f)*tw*3.0;
        double margleft = f*tw*5.0 + (1-f)*tw*6.0;
        double size = f*tw*12.0 + (1-f)*tw*3.0;
        brc.right = (int)(x-margright);
        brc.left = (int)(x-margright-size);
        brc.top = (sav.y1+sav.y2)/2 - (int)size/2;
        brc.bottom = (sav.y1+sav.y2)/2 + (int)size/2;
        SimpleDraw(bufdc,brc,b,0,0);
        x -= (int)(margright+size+margleft);
        lastvisible = (brc.right>0);
        if (i->state==psZoomed)
        { brc.top = brc.bottom*9/10+brc.top*1/10;
          stk::FitText(bufdc,(int)(tw*0.5),&brc,i->d);
        }
      }
    }
    // if the last one was unseen, we can pop it
    if (procession.size()>0 && !lastvisible)
    { TProcessData d=procession.back(); procession.pop_back(); 
      if (zoomin && !gotgap && m==nprim+1) {zoomgap=d; gotgap=true;}
      else if (m>0) {PushItem(d,m-1); PopActive(d.gindex);}
      else PopActive(d.gindex);
    }

    //
    if (tick<bantime && m==nprim)
    { RECT brc; brc.left=0; brc.right=sav.cw; brc.top=sav.ch/10; brc.bottom=sav.ch*2/10;
      stk::FitText(bufdc,(int)(tw*0.5),&brc,banner);
    }
    else if (testparade!=-1 && m==nprim)
    { RECT brc; brc.left=0; brc.right=sav.cw; brc.top=sav.ch/10; brc.bottom=sav.ch*2/10;
      char c[50]; wsprintf(c,"%i/%i",testparade,(int)gbods.size());
      stk::FitText(bufdc,(int)(tw*0.5),&brc,c);
    }

    //TAmpData ad; double l[6],r[6],k[6];
    //for (int i=0; i<6; i++) {l[i]=freq[0][i]; r[i]=freq[1][i]; k[i]=freq[2][i];}
    //ad.l=l; ad.r=l; ad.min=wmin; ad.max=wmax; ad.k=k; ad.kmin=kmin; ad.kmax=kmax;
    //GetClientRect(hwnd,&rc);
    //SimpleDraw(bufdc,rc,0,0,&ad,0);
    BitBlt(hdc,0,0,sav.cw,sav.ch,bufdc,0,0,SRCCOPY);
    DeleteDC(bufdc); ReleaseDC(sav.hwnd,hdc);
  }


}







// WAVEFORM WINDOW.
// This window class appears in the configuration dialog. It displays a trace
// of the current audio, which is stored in a global variable. This window class
// is registered in WinMain.
//
LRESULT CALLBACK WaveformWndProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ switch (msg)
  { case WM_APP: InvalidateRect(hwnd,NULL,TRUE); return 0;
    case WM_PAINT:
    { PAINTSTRUCT ps; BeginPaint(hwnd,&ps);
      const int smax = (buf_nsamples<=256)?buf_nsamples:256; 
      const int symin=srange<sample_t>::smin(), symax=srange<sample_t>::smax(), sh=symax-symin;
      RECT rc; GetClientRect(hwnd,&rc); int cw=rc.right, ch=rc.bottom;
      POINT ptl[smax], ptr[smax];
      if (hwavein==0)
      { for (int i=0; i<smax; i++)
        { ptl[i].x=ptr[i].x=i*cw/smax;
          ptl[i].y=ptr[i].y=(srange<sample_t>::smid()-symin)*ch/sh;
        }
      }
      else
      { for (int i=0; i<smax; i++)
        { ptl[i].x = ptr[i].x = i*cw/smax;
          ptl[i].y=(waveformData[i*nchannels+0]-symin)*ch/sh;
          ptr[i].y=(waveformData[i*nchannels+1]-symin)*ch/sh;
        }
      }
      HPEN hpl=CreatePen(PS_SOLID,0,RGB(0,255,0)), hpr=CreatePen(PS_SOLID,0,RGB(255,0,0));
      HGDIOBJ holdp=SelectObject(ps.hdc,hpl);
      Polyline(ps.hdc,ptl,smax);
      SelectObject(ps.hdc,hpr);
      Polyline(ps.hdc,ptr,smax);
      SelectObject(ps.hdc,holdp); DeleteObject(hpl); DeleteObject(hpr);
      EndPaint(hwnd,&ps);
    } return 0;
  }
  return DefWindowProc(hwnd,msg,wParam,lParam);
}



BOOL CALLBACK OptionsDlgProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ switch (msg)
  { case WM_INITDIALOG:
    { CommonInit();
      CheckDlgButton(hwnd,102,TryAudio?BST_CHECKED:BST_UNCHECKED);
      if (TryAudio) AudioStart(GetDlgItem(hwnd,101));
    } return TRUE;
    case WM_DESTROY:
    { AudioStop(hwnd);
      CommonExit();
    } return TRUE;
    case WM_COMMAND:
    { int id=LOWORD(wParam), code=HIWORD(wParam);
      if (id==102 && code==BN_CLICKED)
      { TryAudio = (IsDlgButtonChecked(hwnd,102)==BST_CHECKED);
        if (TryAudio) AudioStart(GetDlgItem(hwnd,101));
        else {AudioStop(GetDlgItem(hwnd,101)); InvalidateRect(GetDlgItem(hwnd,101),NULL,TRUE);}
      }
    } return TRUE;
    case WM_NOTIFY:
    { LPNMHDR nmh=(LPNMHDR)lParam; UINT code=nmh->code;
      switch (code)
      { case (PSN_APPLY):
        { RegSave(_T("TryAudio"),TryAudio);
          SetWindowLong(hwnd,DWL_MSGRESULT,PSNRET_NOERROR);
        } return TRUE;
      }
    } return FALSE;
  }
  return FALSE;
}




BOOL CALLBACK SticksDlgProc(HWND hwnd,UINT msg,WPARAM,LPARAM)
{ switch (msg)
  { case WM_INITDIALOG:
	  { CommonInit();
      TCHAR c[1024];
      GetDlgItemText(hwnd,102,c,1024); SetDlgItemUrl(hwnd,102,c);
      GetDlgItemText(hwnd,103,c,1024); SetDlgItemUrl(hwnd,103,c);
      for (vector<TBodyInfo>::const_iterator i=gbods.begin(); i!=gbods.end(); i++)
      { SendDlgItemMessage(hwnd,101,LB_ADDSTRING,0,(LPARAM)i->desc.c_str());
      }
    } return TRUE;
    case WM_DESTROY:
    { CommonExit();
    }
  }
  return FALSE;
}



// ---------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------

BOOL VerifyPassword(HWND hwnd)
{ // Under NT, we return TRUE immediately. This lets the saver quit, and the system manages passwords.
  // Under '95, we call VerifyScreenSavePwd. This checks the appropriate registry key and, if necessary, pops up a verify dialog
  OSVERSIONINFO osv; osv.dwOSVersionInfoSize=sizeof(osv); GetVersionEx(&osv);
  if (osv.dwPlatformId==VER_PLATFORM_WIN32_NT) return TRUE;
  HINSTANCE hpwdcpl=::LoadLibrary(_T("PASSWORD.CPL"));
  if (hpwdcpl==NULL) {Debug(_T("Unable to load PASSWORD.CPL. Aborting"));return TRUE;}
  typedef BOOL (WINAPI *VERIFYSCREENSAVEPWD)(HWND hwnd);
  VERIFYSCREENSAVEPWD VerifyScreenSavePwd;
  VerifyScreenSavePwd=(VERIFYSCREENSAVEPWD)GetProcAddress(hpwdcpl,"VerifyScreenSavePwd");
  if (VerifyScreenSavePwd==NULL) {Debug(_T("Unable to get VerifyPwProc address. Aborting"));FreeLibrary(hpwdcpl);return TRUE;}
  Debug(_T("About to call VerifyPwProc")); BOOL bres=VerifyScreenSavePwd(hwnd); FreeLibrary(hpwdcpl);
  return bres;
}
void ChangePassword(HWND hwnd)
{ // This only ever gets called under '95, when started with the /a option.
  HINSTANCE hmpr=::LoadLibrary(_T("MPR.DLL"));
  if (hmpr==NULL) {Debug(_T("MPR.DLL not found: cannot change password."));return;}
  typedef VOID (WINAPI *PWDCHANGEPASSWORD) (LPCSTR lpcRegkeyname,HWND hwnd,UINT uiReserved1,UINT uiReserved2);
  PWDCHANGEPASSWORD PwdChangePassword=(PWDCHANGEPASSWORD)::GetProcAddress(hmpr,"PwdChangePasswordA");
  if (PwdChangePassword==NULL) {FreeLibrary(hmpr); Debug(_T("PwdChangeProc not found: cannot change password"));return;}
  PwdChangePassword("SCRSAVE",hwnd,0,0); FreeLibrary(hmpr);
}



void ReadGeneralRegistry()
{ PasswordDelay=15; PasswordDelayIndex=0; MouseThreshold=50; IsDialogActive=FALSE; // default values in case they're not in registry
  LONG res; HKEY skey; DWORD valtype, valsize, val;
  res=RegOpenKeyEx(HKEY_CURRENT_USER,REGSTR_PATH_SETUP _T("\\Screen Savers"),0,KEY_READ,&skey);
  if (res!=ERROR_SUCCESS) return;
  valsize=sizeof(val); res=RegQueryValueEx(skey,_T("Password Delay"),0,&valtype,(LPBYTE)&val,&valsize); if (res==ERROR_SUCCESS) PasswordDelay=val;
  valsize=sizeof(val); res=RegQueryValueEx(skey,_T("Password Delay Index"),0,&valtype,(LPBYTE)&val,&valsize); if (res==ERROR_SUCCESS) PasswordDelayIndex=val;
  valsize=sizeof(val); res=RegQueryValueEx(skey,_T("Mouse Threshold"),0,&valtype,(LPBYTE)&val,&valsize);if (res==ERROR_SUCCESS) MouseThreshold=val;
  valsize=sizeof(val); res=RegQueryValueEx(skey,_T("Mouse Threshold Index"),0,&valtype,(LPBYTE)&val,&valsize);if (res==ERROR_SUCCESS) MouseThresholdIndex=val;
  valsize=sizeof(val); res=RegQueryValueEx(skey,_T("Mute Sound"),0,&valtype,(LPBYTE)&val,&valsize);     if (res==ERROR_SUCCESS) MuteSound=val;
  valsize=5*sizeof(TCHAR); RegQueryValueEx(skey,_T("Mouse Corners"),0,&valtype,(LPBYTE)Corners,&valsize);
  for (int i=0; i<4; i++) {if (Corners[i]!='Y' && Corners[i]!='N') Corners[i]='-';} Corners[4]=0;
  RegCloseKey(skey);
  //
  HotServices=FALSE;
  HINSTANCE sagedll=LoadLibrary(_T("sage.dll"));
	if (sagedll==0) sagedll=LoadLibrary(_T("scrhots.dll"));
  if (sagedll!=0)
  { typedef BOOL (WINAPI *SYSTEMAGENTDETECT)();
    SYSTEMAGENTDETECT detectproc=(SYSTEMAGENTDETECT)GetProcAddress(sagedll,"System_Agent_Detect");
    if (detectproc!=NULL) HotServices=detectproc();
    FreeLibrary(sagedll);
  }
}

void WriteGeneralRegistry()
{ LONG res; HKEY skey; DWORD val;
  res=RegCreateKeyEx(HKEY_CURRENT_USER,REGSTR_PATH_SETUP _T("\\Screen Savers"),0,0,0,KEY_ALL_ACCESS,0,&skey,0);
  if (res!=ERROR_SUCCESS) return;
  val=PasswordDelay; RegSetValueEx(skey,_T("Password Delay"),0,REG_DWORD,(LPCBYTE)&val,sizeof(val));
  val=PasswordDelayIndex; RegSetValueEx(skey,_T("Password Delay Index"),0,REG_DWORD,(LPCBYTE)&val,sizeof(val));
  val=MouseThreshold; RegSetValueEx(skey,_T("Mouse Threshold"),0,REG_DWORD,(LPCBYTE)&val,sizeof(val));
  val=MouseThresholdIndex; RegSetValueEx(skey,_T("Mouse Threshold Index"),0,REG_DWORD,(LPCBYTE)&val,sizeof(val));
  val=MuteSound?1:0; RegSetValueEx(skey,_T("Mute Sound"),0,REG_DWORD,(LPCBYTE)&val,sizeof(val));
  RegSetValueEx(skey,_T("Mouse Corners"),0,REG_SZ,(LPCBYTE)Corners,5*sizeof(TCHAR));
  RegCloseKey(skey);
  //
  HINSTANCE sagedll=LoadLibrary(_T("sage.dll"));
	if (sagedll==0) sagedll=LoadLibrary(_T("scrhots.dll"));
  if (sagedll!=0)
  { typedef VOID (WINAPI *SCREENSAVERCHANGED)();
    SCREENSAVERCHANGED changedproc=(SCREENSAVERCHANGED)GetProcAddress(sagedll,"Screen_Saver_Changed");
    if (changedproc!=NULL) changedproc();
    FreeLibrary(sagedll);
  }
}


  
LRESULT CALLBACK SaverWndProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ TSaverWindow *sav; int id; HWND hmain;
  #pragma warning( push )
  #pragma warning( disable: 4244 4312 )
  if (msg==WM_CREATE)
  { CREATESTRUCT *cs=(CREATESTRUCT*)lParam; id=*(int*)cs->lpCreateParams; SetWindowLongPtr(hwnd,0,id);
    sav = new TSaverWindow(hwnd,id); SetWindowLongPtr(hwnd,GWLP_USERDATA,(LONG_PTR)sav);
    SaverWindow.push_back(sav);
  }
  else
  { sav=(TSaverWindow*)GetWindowLongPtr(hwnd,GWLP_USERDATA);
    id=GetWindowLongPtr(hwnd,0);
  }
  #pragma warning( pop )
  if (id<=0) hmain=hwnd; else hmain=SaverWindow[0]->hwnd;
  //
  if (msg==WM_TIMER) sav->OnTimer();
  else if (msg==WM_PAINT) {PAINTSTRUCT ps; BeginPaint(hwnd,&ps); RECT rc; GetClientRect(hwnd,&rc); if (sav!=0) sav->OnPaint(ps.hdc,rc); EndPaint(hwnd,&ps);}
  else if (sav!=0) sav->OtherWndProc(msg,wParam,lParam);
  //
  switch (msg)
  { case WM_ACTIVATE: case WM_ACTIVATEAPP: case WM_NCACTIVATE:
    { TCHAR pn[100]; GetClassName((HWND)lParam,pn,100); 
      bool ispeer = (_tcsncmp(pn,_T("ScrClass"),8)==0);
      if (ScrMode==smSaver && !IsDialogActive && LOWORD(wParam)==WA_INACTIVE && !ispeer && !SCRDEBUG)
      { Debug(_T("WM_ACTIVATE: about to inactive window, so sending close")); ReallyClose=true; PostMessage(hmain,WM_CLOSE,0,0); return 0;
      }
    } break;
    case WM_SETCURSOR:
    { if (ScrMode==smSaver && !IsDialogActive && !SCRDEBUG) SetCursor(NULL);
      else SetCursor(LoadCursor(NULL,IDC_ARROW));
    } return 0;
    case WM_LBUTTONDOWN: case WM_MBUTTONDOWN: case WM_RBUTTONDOWN: case WM_KEYDOWN:
    { if (ScrMode==smSaver && !IsDialogActive) {Debug(_T("WM_BUTTONDOWN: sending close")); ReallyClose=true; PostMessage(hmain,WM_CLOSE,0,0); return 0;}
    } break;
    case WM_MOUSEMOVE:
    { if (ScrMode==smSaver && !IsDialogActive && !SCRDEBUG)
      { POINT pt; GetCursorPos(&pt);
        int dx=pt.x-InitCursorPos.x; if (dx<0) dx=-dx; int dy=pt.y-InitCursorPos.y; if (dy<0) dy=-dy;
        if (dx>(int)MouseThreshold || dy>(int)MouseThreshold)
        { Debug(_T("WM_MOUSEMOVE: gone beyond threshold, sending close")); ReallyClose=true; PostMessage(hmain,WM_CLOSE,0,0);
        }
      }
    } return 0;
    case (WM_SYSCOMMAND):
    { if (ScrMode==smSaver)
      { if (wParam==SC_SCREENSAVE) {Debug(_T("WM_SYSCOMMAND: gobbling up a SC_SCREENSAVE to stop a new saver from running."));return 0;}
        if (wParam==SC_CLOSE && !SCRDEBUG) {Debug(_T("WM_SYSCOMMAND: gobbling up a SC_CLOSE"));return 0;}
      }
    } break;
    case (WM_CLOSE):
    { if (id>0) return 0; // secondary windows ignore this message
      if (id==-1) {DestroyWindow(hwnd); return 0;} // preview windows close obediently
      if (ReallyClose && !IsDialogActive)
      { Debug(_T("WM_CLOSE: maybe we need a password"));
        BOOL CanClose=TRUE;
        if (GetTickCount()-InitTime > 1000*PasswordDelay)
        { IsDialogActive=true; SendMessage(hwnd,WM_SETCURSOR,0,0);
          CanClose=VerifyPassword(hwnd);
          IsDialogActive=false; SendMessage(hwnd,WM_SETCURSOR,0,0); GetCursorPos(&InitCursorPos);
        }
        // note: all secondary monitors are owned by the primary. And we're the primary (id==0)
        // therefore, when we destroy ourself, windows will destroy the others first
        if (CanClose) {Debug(_T("WM_CLOSE: doing a DestroyWindow")); DestroyWindow(hwnd);}
        else {Debug(_T("WM_CLOSE: but failed password, so doing nothing"));}
      }
    } return 0;
    case (WM_DESTROY):
    { Debug(_T("WM_DESTROY."));
      SetWindowLong(hwnd,0,0); SetWindowLong(hwnd,GWL_USERDATA,0);
      for (vector<TSaverWindow*>::iterator i=SaverWindow.begin(); i!=SaverWindow.end(); i++) {if (sav==*i) *i=0;}
      delete sav;
      if ((id==0 && ScrMode==smSaver) || ScrMode==smPreview) PostQuitMessage(0);
    } break;
  }
  return DefWindowProc(hwnd,msg,wParam,lParam);
}





// ENUM-MONITOR-CALLBACK is part of DoSaver.
BOOL CALLBACK EnumMonitorCallback(HMONITOR,HDC,LPRECT rc,LPARAM)
{ if (rc->left==0 && rc->top==0) monitors.insert(monitors.begin(),*rc);
  else monitors.push_back(*rc);
  return TRUE;
}

void DoSaver(HWND hparwnd, bool fakemulti)
{ if (ScrMode==smPreview)
  { RECT rc; GetWindowRect(hparwnd,&rc); monitors.push_back(rc);
  }
  else if (fakemulti)
  { int w=GetSystemMetrics(SM_CXSCREEN), x1=w/4, x2=w*2/3, h=x2-x1; RECT rc;
    rc.left=x1; rc.top=x1; rc.right=x1+h; rc.bottom=x1+h; monitors.push_back(rc);
    rc.left=0; rc.top=x1; rc.right=x1; rc.bottom=x1+x1; monitors.push_back(rc);
    rc.left=x2; rc.top=x1+h+x2-w; rc.right=w; rc.bottom=x1+h; monitors.push_back(rc);
  }
  else
  { int num_monitors=GetSystemMetrics(SM_CMONITORS);
    if (num_monitors>1)
    { typedef BOOL (CALLBACK *LUMONITORENUMPROC)(HMONITOR,HDC,LPRECT,LPARAM);
      typedef BOOL (WINAPI *LUENUMDISPLAYMONITORS)(HDC,LPCRECT,LUMONITORENUMPROC,LPARAM);
      HINSTANCE husr=LoadLibrary(_T("user32.dll"));
      LUENUMDISPLAYMONITORS pEnumDisplayMonitors=0;
      if (husr!=NULL) pEnumDisplayMonitors=(LUENUMDISPLAYMONITORS)GetProcAddress(husr,"EnumDisplayMonitors");
      if (pEnumDisplayMonitors!=NULL) (*pEnumDisplayMonitors)(NULL,NULL,EnumMonitorCallback,NULL);
      if (husr!=NULL) FreeLibrary(husr);
    }
    if (monitors.size()==0)
    { RECT rc; rc.left=0; rc.top=0; rc.right=GetSystemMetrics(SM_CXSCREEN); rc.bottom=GetSystemMetrics(SM_CYSCREEN);
      monitors.push_back(rc);
    }
  }
  //
  HWND hwnd=0;
  if (ScrMode==smPreview)
  { RECT rc; GetWindowRect(hparwnd,&rc); int w=rc.right-rc.left, h=rc.bottom-rc.top;  
    int id=-1; hwnd=CreateWindowEx(0,_T("ScrClass"),_T(""),WS_CHILD|WS_VISIBLE,0,0,w,h,hparwnd,NULL,hInstance,&id);
  }
  else
  { GetCursorPos(&InitCursorPos); InitTime=GetTickCount();
    for (int i=0; i<(int)monitors.size(); i++)
    { const RECT &rc = monitors[i];
      DWORD exstyle=WS_EX_TOPMOST; if (SCRDEBUG) exstyle=0;
      HWND hthis = CreateWindowEx(exstyle,_T("ScrClass"),_T(""),WS_POPUP|WS_VISIBLE,rc.left,rc.top,rc.right-rc.left,rc.bottom-rc.top,hwnd,NULL,hInstance,&i);
      if (i==0) hwnd=hthis;
    }
  }
  if (hwnd!=NULL)
  { UINT oldval;
    if (ScrMode==smSaver) SystemParametersInfo(SPI_SETSCREENSAVERRUNNING,1,&oldval,0);
    MSG msg;
    while (GetMessage(&msg,NULL,0,0))
    { TranslateMessage(&msg);
      DispatchMessage(&msg);
    }
    if (ScrMode==smSaver) SystemParametersInfo(SPI_SETSCREENSAVERRUNNING,0,&oldval,0);
  }
  //
  SaverWindow.clear();
  return;
}




BOOL CALLBACK GeneralDlgProc(HWND hwnd,UINT msg,WPARAM,LPARAM lParam)
{ switch (msg)
  { case (WM_INITDIALOG):
	  { ShowWindow(GetDlgItem(hwnd,HotServices?102:101),SW_HIDE);
      SetDlgItemText(hwnd,112,Corners);
      SendDlgItemMessage(hwnd,109,CB_ADDSTRING,0,(LPARAM)_T("seconds"));
      SendDlgItemMessage(hwnd,109,CB_ADDSTRING,0,(LPARAM)_T("minutes"));
      SendDlgItemMessage(hwnd,109,CB_SETCURSEL,PasswordDelayIndex,0);
      int n=PasswordDelay; if (PasswordDelayIndex==1) n/=60;
      TCHAR c[16]; wsprintf(c,_T("%i"),n); SetDlgItemText(hwnd,107,c);
      SendDlgItemMessage(hwnd,108,UDM_SETRANGE,0,MAKELONG(99,0));
      SendDlgItemMessage(hwnd,105,CB_ADDSTRING,0,(LPARAM)_T("High"));
      SendDlgItemMessage(hwnd,105,CB_ADDSTRING,0,(LPARAM)_T("Normal"));
      SendDlgItemMessage(hwnd,105,CB_ADDSTRING,0,(LPARAM)_T("Low"));
      SendDlgItemMessage(hwnd,105,CB_ADDSTRING,0,(LPARAM)_T("Keyboard only (ignore mouse movement)"));
      SendDlgItemMessage(hwnd,105,CB_SETCURSEL,MouseThresholdIndex,0);
      if (MuteSound) CheckDlgButton(hwnd,113,BST_CHECKED);
      OSVERSIONINFO ver; ZeroMemory(&ver,sizeof(ver)); ver.dwOSVersionInfoSize=sizeof(ver); GetVersionEx(&ver);
      for (int i=106; i<111 && ver.dwPlatformId!=VER_PLATFORM_WIN32_WINDOWS; i++) ShowWindow(GetDlgItem(hwnd,i),SW_HIDE); 
    } return TRUE;
    case (WM_NOTIFY):
    { LPNMHDR nmh=(LPNMHDR)lParam; UINT code=nmh->code;
      switch (code)
      { case (PSN_APPLY):
        { GetDlgItemText(hwnd,112,Corners,5);                   
          PasswordDelayIndex=SendDlgItemMessage(hwnd,109,CB_GETCURSEL,0,0);
          TCHAR c[16]; GetDlgItemText(hwnd,107,c,16); int n=_ttoi(c); if (PasswordDelayIndex==1) n*=60; PasswordDelay=n;
          MouseThresholdIndex=SendDlgItemMessage(hwnd,105,CB_GETCURSEL,0,0);
          if (MouseThresholdIndex==0) MouseThreshold=0;
          else if (MouseThresholdIndex==1) MouseThreshold=200;
          else if (MouseThresholdIndex==2) MouseThreshold=400;
          else MouseThreshold=999999;
          MuteSound = (IsDlgButtonChecked(hwnd,113)==BST_CHECKED);
          WriteGeneralRegistry();
          SetWindowLong(hwnd,DWL_MSGRESULT,PSNRET_NOERROR);
        } return TRUE;
      }
    } return FALSE;
  }
  return FALSE;
}



//
// MONITOR CONTROL -- either corners or a preview
//

LRESULT CALLBACK MonitorWndProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ switch (msg)
  { case WM_CREATE:
    { TCHAR c[5]; GetWindowText(hwnd,c,5); if (*c!=0) return 0;
      int id=-1; RECT rc; SendMessage(hwnd,SCRM_GETMONITORAREA,0,(LPARAM)&rc);
      CreateWindow(_T("ScrClass"),_T(""),WS_CHILD|WS_VISIBLE,rc.left,rc.top,rc.right-rc.left,rc.bottom-rc.top,hwnd,NULL,hInstance,&id);
    } return 0;
    case WM_PAINT:
    { if (hbmmonitor==0) hbmmonitor=LoadBitmap(hInstance,_T("Monitor"));
      RECT rc; GetClientRect(hwnd,&rc);
      //
      PAINTSTRUCT ps; BeginPaint(hwnd,&ps);
      HBITMAP hback = (HBITMAP)GetWindowLong(hwnd,GWL_USERDATA);
      if (hback!=0)
      { BITMAP bmp; GetObject(hback,sizeof(bmp),&bmp);
        if (bmp.bmWidth!=rc.right || bmp.bmHeight!=rc.bottom) {DeleteObject(hback); hback=0;}
      }
      if (hback==0) {hback=CreateCompatibleBitmap(ps.hdc,rc.right,rc.bottom); SetWindowLongPtr(hwnd,GWLP_USERDATA,(LONG_PTR)hback);}
      HDC backdc=CreateCompatibleDC(ps.hdc);
      HGDIOBJ holdback=SelectObject(backdc,hback);
      BitBlt(backdc,0,0,rc.right,rc.bottom,ps.hdc,0,0,SRCCOPY);
      //
      TCHAR corners[5]; GetWindowText(hwnd,corners,5);
      HDC hdc=CreateCompatibleDC(ps.hdc);
      HGDIOBJ hold=SelectObject(hdc,hbmmonitor);
      StretchBlt(backdc,0,0,rc.right,rc.bottom,hdc,0,0,184,170,SRCAND);
      StretchBlt(backdc,0,0,rc.right,rc.bottom,hdc,184,0,184,170,SRCINVERT);
      RECT crc; SendMessage(hwnd,SCRM_GETMONITORAREA,0,(LPARAM)&crc);
      //
      if (*corners!=0) FillRect(backdc,&crc,GetSysColorBrush(COLOR_DESKTOP));
      for (int i=0; i<4 && *corners!=0; i++)
      { RECT crc; SendMessage(hwnd,SCRM_GETMONITORAREA,i+1,(LPARAM)&crc);
        int y=0; if (corners[i]=='Y') y=22; else if (corners[i]=='N') y=44;
        BitBlt(backdc,crc.left,crc.top,crc.right-crc.left,crc.bottom-crc.top,hdc,368,y,SRCCOPY);
        if (!HotServices) 
        { DWORD col=GetSysColor(COLOR_DESKTOP);
          for (int y=crc.top; y<crc.bottom; y++)
          { for (int x=crc.left+(y&1); x<crc.right; x+=2) SetPixel(backdc,x,y,col);
          }
        }
      }
      SelectObject(hdc,hold);
      DeleteDC(hdc);
      //
      BitBlt(ps.hdc,0,0,rc.right,rc.bottom,backdc,0,0,SRCCOPY);
      SelectObject(backdc,holdback);
      DeleteDC(backdc);
      EndPaint(hwnd,&ps);
    } return 0;
    case SCRM_GETMONITORAREA:
    { RECT *prc=(RECT*)lParam;
      if (hbmmonitor==0) hbmmonitor=LoadBitmap(hInstance,_T("Monitor"));
      // those are the client coordinates unscalled
      RECT wrc; GetClientRect(hwnd,&wrc); int ww=wrc.right, wh=wrc.bottom;
      RECT rc; rc.left=16*ww/184; rc.right=168*ww/184; rc.top=17*wh/170; rc.bottom=130*wh/170;
      *prc=rc; if (wParam==0) return 0;
      if (wParam==1) {prc->right=rc.left+24; prc->bottom=rc.top+22;}
      else if (wParam==2) {prc->left=rc.right-24; prc->bottom=rc.top+22;}
      else if (wParam==3) {prc->left=rc.right-24; prc->top=rc.bottom-22;}
      else if (wParam==4) {prc->right=rc.left+24; prc->top=rc.bottom-22;}
    } return 0;
    case WM_LBUTTONDOWN:
    { if (!HotServices) return 0;
      int x=LOWORD(lParam), y=HIWORD(lParam);
      TCHAR corners[5]; GetWindowText(hwnd,corners,5);
      if (corners[0]==0) return 0;
      int click=-1; for (int i=0; i<4; i++)
      { RECT rc; SendMessage(hwnd,SCRM_GETMONITORAREA,i+1,(LPARAM)&rc);
        if (x>=rc.left && y>=rc.top && x<rc.right && y<rc.bottom) {click=i; break;}
      }
      if (click==-1) return 0;
      for (int i=0; i<4; i++)
      { if (corners[i]!='-' && corners[i]!='Y' && corners[i]!='N') corners[i]='-';
      }
      corners[4]=0;
      //
      HMENU hmenu=CreatePopupMenu();
      MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(MENUITEMINFO);
      mi.fMask=MIIM_TYPE|MIIM_ID|MIIM_STATE|MIIM_DATA;
      mi.fType=MFT_STRING|MFT_RADIOCHECK;
      mi.wID='N'; mi.fState=MFS_ENABLED; if (corners[click]=='N') mi.fState|=MFS_CHECKED;
      mi.dwTypeData=_T("Never"); mi.cch=sizeof(TCHAR)*_tcslen(mi.dwTypeData);
      InsertMenuItem(hmenu,0,TRUE,&mi);
      mi.wID='Y'; mi.fState=MFS_ENABLED; if (corners[click]=='Y') mi.fState|=MFS_CHECKED;
      mi.dwTypeData=_T("Now"); mi.cch=sizeof(TCHAR)*_tcslen(mi.dwTypeData);
      InsertMenuItem(hmenu,0,TRUE,&mi);
      mi.wID='-'; mi.fState=MFS_ENABLED; if (corners[click]!='Y' && corners[click]!='N') mi.fState|=MFS_CHECKED;
      mi.dwTypeData=_T("Default"); mi.cch=sizeof(TCHAR)*_tcslen(mi.dwTypeData);
      InsertMenuItem(hmenu,0,TRUE,&mi);
      POINT pt; pt.x=x; pt.y=y; ClientToScreen(hwnd,&pt);
      int cmd = TrackPopupMenuEx(hmenu,TPM_RETURNCMD|TPM_RIGHTBUTTON,pt.x,pt.y,hwnd,NULL);
      if (cmd!=0) corners[click]=(char)cmd;
      corners[4]=0; SetWindowText(hwnd,corners);
      InvalidateRect(hwnd,NULL,FALSE);
    } return 0;
    case WM_DESTROY:
    { HBITMAP hback = (HBITMAP)SetWindowLongPtr(hwnd,GWL_USERDATA,0);
      if (hback!=0) DeleteObject(hback);
    } return 0;
  }
  return DefWindowProc(hwnd, msg, wParam, lParam);
}






BOOL CALLBACK AboutDlgProc(HWND hdlg, UINT msg, WPARAM wParam, LPARAM)
{ if (msg==WM_INITDIALOG)
  { SetDlgItemText(hdlg,101,SaverName.c_str());
    SetDlgItemUrl(hdlg,102,_T("http://www.wischik.com/lu/"));
    SetDlgItemText(hdlg,102,_T("www.wischik.com/lu"));
    return TRUE;
  }
  else if (msg==WM_COMMAND)
  { int id=LOWORD(wParam);
    if (id==IDOK || id==IDCANCEL) EndDialog(hdlg,id);
    return TRUE;
  } 
  else return FALSE;
}


//
// PROPERTY SHEET SUBCLASSING -- this is to stick an "About" option on the sysmenu.
//
WNDPROC OldSubclassProc=0;
LRESULT CALLBACK PropertysheetSubclassProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ if (msg==WM_SYSCOMMAND && wParam==3500)
  { DialogBox(hInstance,_T("DLG_ABOUT"),hwnd,AboutDlgProc);
    return 0;
  } 
  if (OldSubclassProc!=NULL) return CallWindowProc(OldSubclassProc,hwnd,msg,wParam,lParam);
  else return DefWindowProc(hwnd,msg,wParam,lParam);
}

int CALLBACK PropertysheetCallback(HWND hwnd,UINT msg,LPARAM)
{ if (msg!=PSCB_INITIALIZED) return 0;
  HMENU hsysmenu=GetSystemMenu(hwnd,FALSE);
  AppendMenu(hsysmenu,MF_SEPARATOR,1,_T("-"));
  AppendMenu(hsysmenu,MF_STRING,3500,_T("About..."));
  OldSubclassProc=(WNDPROC)SetWindowLongPtr(hwnd,GWLP_WNDPROC,(LONG_PTR)PropertysheetSubclassProc);
  return 0;
}


void DoConfig(HWND hpar)
{ hiconbg = (HICON)LoadImage(hInstance,MAKEINTRESOURCE(1),IMAGE_ICON,GetSystemMetrics(SM_CXICON),GetSystemMetrics(SM_CYICON),0);
  hiconsm = (HICON)LoadImage(hInstance,MAKEINTRESOURCE(1),IMAGE_ICON,GetSystemMetrics(SM_CXSMICON),GetSystemMetrics(SM_CYSMICON),0);
  //  
  PROPSHEETHEADER psh; ZeroMemory(&psh,sizeof(psh));
  PROPSHEETPAGE psp[3]; ZeroMemory(psp,3*sizeof(PROPSHEETPAGE));
  psp[0].dwSize=sizeof(psp[0]);
  psp[0].dwFlags=PSP_DEFAULT;
  psp[0].hInstance=hInstance; 
  psp[0].pszTemplate=_T("DLG_GENERAL");
  psp[0].pfnDlgProc=GeneralDlgProc;
  psp[1].dwSize=sizeof(psp[1]);
  psp[1].dwFlags=PSP_DEFAULT;
  psp[1].hInstance=hInstance;  
  psp[1].pszTemplate=_T("DLG_OPTIONS");
  psp[1].pfnDlgProc=OptionsDlgProc;
  psp[2].dwSize=sizeof(psp[2]);
  psp[2].dwFlags=PSP_DEFAULT;
  psp[2].hInstance=hInstance;
  psp[2].pszTemplate=_T("DLG_STICKS");
  psp[2].pfnDlgProc=SticksDlgProc;
  psh.dwSize=sizeof(psh);
  psh.dwFlags=PSH_NOAPPLYNOW | PSH_PROPSHEETPAGE | PSH_USEHICON | PSH_USECALLBACK;
  psh.hwndParent=hpar;
  psh.hInstance=hInstance;
  psh.hIcon=hiconsm;
  tstring cap=_T("Options for ")+SaverName; psh.pszCaption=cap.c_str();
  psh.nPages=3;
  psh.nStartPage=1;
  psh.ppsp=psp;
  psh.pfnCallback=PropertysheetCallback;
  Debug(_T("Config..."));
  PropertySheet(&psh);
  Debug(_T("Config done."));
  if (hiconbg!=0) DestroyIcon(hiconbg); hiconbg=0;
  if (hiconsm!=0) DestroyIcon(hiconsm); hiconsm=0;
  if (hbmmonitor!=0) DeleteObject(hbmmonitor); hbmmonitor=0;
}


// This routine is for using ScrPrev. It's so that you can start the saver
// with the command line /p scrprev and it runs itself in a preview window.
// You must first copy ScrPrev somewhere in your search path
HWND CheckForScrprev()
{ HWND hwnd=FindWindow(_T("Scrprev"),NULL); // looks for the Scrprev class
  if (hwnd==NULL) // try to load it
  { STARTUPINFO si; PROCESS_INFORMATION pi; ZeroMemory(&si,sizeof(si)); ZeroMemory(&pi,sizeof(pi));
    si.cb=sizeof(si);
    TCHAR cmd[MAX_PATH]; _tcscpy(cmd,_T("Scrprev")); // unicode CreateProcess requires it writeable
    BOOL cres=CreateProcess(NULL,cmd,0,0,FALSE,CREATE_NEW_PROCESS_GROUP | CREATE_DEFAULT_ERROR_MODE,
                            0,0,&si,&pi);
    if (!cres) {Debug(_T("Error creating scrprev process")); return NULL;}
    DWORD wres=WaitForInputIdle(pi.hProcess,2000);
    if (wres==WAIT_TIMEOUT) {Debug(_T("Scrprev never becomes idle")); return NULL;}
    if (wres==0xFFFFFFFF) {Debug(_T("ScrPrev, misc error after ScrPrev execution"));return NULL;}
    hwnd=FindWindow(_T("Scrprev"),NULL);
  }
  if (hwnd==NULL) {Debug(_T("Unable to find Scrprev window")); return NULL;}
  ::SetForegroundWindow(hwnd);
  hwnd=GetWindow(hwnd,GW_CHILD);
  if (hwnd==NULL) {Debug(_T("Couldn't find Scrprev child")); return NULL;}
  return hwnd;
}


void DoInstall()
{ TCHAR windir[MAX_PATH]; GetWindowsDirectory(windir,MAX_PATH);
  TCHAR tfn[MAX_PATH]; UINT ures=GetTempFileName(windir,_T("pst"),0,tfn);
  if (ures==0) {MessageBox(NULL,_T("You must be logged on as system administrator to install screen savers"),_T("Saver Install"),MB_ICONINFORMATION|MB_OK); return;}
  DeleteFile(tfn);
  tstring fn=tstring(windir)+_T("\\")+SaverName+_T(".scr");
  DWORD attr = GetFileAttributes(fn.c_str());
  bool exists = (attr!=0xFFFFFFFF);
  tstring msg=_T("Do you want to install '")+SaverName+_T("' ?");
  if (exists) msg+=_T("\r\n\r\n(This will replace the version that you have currently)");
  int res=MessageBox(NULL,msg.c_str(),_T("Saver Install"),MB_YESNOCANCEL);
  if (res!=IDYES) return;
  TCHAR cfn[MAX_PATH]; GetModuleFileName(hInstance,cfn,MAX_PATH);
  SetCursor(LoadCursor(NULL,IDC_WAIT));
  BOOL bres = CopyFile(cfn,fn.c_str(),FALSE);
  if (!bres)
  { tstring msg = _T("There was an error installing the saver.\r\n\r\n\"")+GetLastErrorString()+_T("\"");
    MessageBox(NULL,msg.c_str(),_T("Saver Install"),MB_ICONERROR|MB_OK);
    SetCursor(LoadCursor(NULL,IDC_ARROW));
    return;
  }
  LONG lres; HKEY skey; DWORD disp; tstring val;
  tstring key=REGSTR_PATH_UNINSTALL _T("\\")+SaverName;
  lres=RegCreateKeyEx(HKEY_LOCAL_MACHINE,key.c_str(),0,NULL,REG_OPTION_NON_VOLATILE,KEY_ALL_ACCESS,NULL,&skey,&disp);
  if (lres==ERROR_SUCCESS)
  { val=SaverName+_T(" saver"); RegSetValueEx(skey,_T("DisplayName"),0,REG_SZ,(const BYTE*)val.c_str(),sizeof(TCHAR)*(val.length()+1));
    val=_T("\"")+fn+_T("\" /u"); RegSetValueEx(skey,_T("UninstallString"),0,REG_SZ,(const BYTE*)val.c_str(),sizeof(TCHAR)*(val.length()+1));
    RegSetValueEx(skey,_T("UninstallPath"),0,REG_SZ,(const BYTE*)val.c_str(),sizeof(TCHAR)*(val.length()+1));
    val=_T("\"")+fn+_T("\""); RegSetValueEx(skey,_T("ModifyPath"),0,REG_SZ,(const BYTE*)val.c_str(),sizeof(TCHAR)*(val.length()+1));
    val=fn; RegSetValueEx(skey,_T("DisplayIcon"),0,REG_SZ,(const BYTE*)val.c_str(),sizeof(TCHAR)*(val.length()+1));
    TCHAR url[1024]; int ures=LoadString(hInstance,2,url,1024); if (ures!=0) RegSetValueEx(skey,_T("HelpLink"),0,REG_SZ,(const BYTE*)url,sizeof(TCHAR)*(_tcslen(url)+1));
    RegCloseKey(skey);
  }
  SHELLEXECUTEINFO sex; ZeroMemory(&sex,sizeof(sex)); sex.cbSize=sizeof(sex);
  sex.fMask=SEE_MASK_NOCLOSEPROCESS;
  sex.lpVerb=_T("install");
  sex.lpFile=fn.c_str();
  sex.nShow=SW_SHOWNORMAL;
  bres = ShellExecuteEx(&sex);
  if (!bres) {SetCursor(LoadCursor(NULL,IDC_ARROW)); MessageBox(NULL,_T("The saver has been installed"),SaverName.c_str(),MB_OK); return;}
  WaitForInputIdle(sex.hProcess,2000);
  CloseHandle(sex.hProcess);
  SetCursor(LoadCursor(NULL,IDC_ARROW));
}


void DoUninstall()
{ tstring key=REGSTR_PATH_UNINSTALL _T("\\")+SaverName;
  RegDeleteKey(HKEY_LOCAL_MACHINE,key.c_str());
  TCHAR fn[MAX_PATH]; GetModuleFileName(hInstance,fn,MAX_PATH);
  SetFileAttributes(fn,FILE_ATTRIBUTE_NORMAL); // remove readonly if necessary
  BOOL res = MoveFileEx(fn,NULL,MOVEFILE_DELAY_UNTIL_REBOOT);
  //
  const TCHAR *c=fn, *lastslash=c;
  while (*c!=0) {if (*c=='\\' || *c=='/') lastslash=c+1; c++;}
  tstring cap=SaverName+_T(" uninstaller");
  tstring msg;
  if (res) msg=_T("Uninstall completed. The saver will be removed next time you reboot.");
  else msg=_T("There was a problem uninstalling.\r\n")
           _T("To complete the uninstall manually, you should go into your Windows ")
           _T("directory and delete the file '")+tstring(lastslash)+_T("'");
  MessageBox(NULL,msg.c_str(),cap.c_str(),MB_OK);
}




// --------------------------------------------------------------------------------
// SetDlgItemUrl(hwnd,IDC_MYSTATIC,"http://www.wischik.com/lu");
//   This routine turns a dialog's static text control into an underlined hyperlink.
//   You can call it in your WM_INITDIALOG, or anywhere.
//   It will also set the text of the control... if you want to change the text
//   back, you can just call SetDlgItemText() afterwards.
// --------------------------------------------------------------------------------
void SetDlgItemUrl(HWND hdlg,int id,const TCHAR *url); 

// Implementation notes:
// We have to subclass both the static control (to set its cursor, to respond to click)
// and the dialog procedure (to set the font of the static control). Here are the two
// subclasses:
LRESULT CALLBACK UrlCtlProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam);
LRESULT CALLBACK UrlDlgProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam);
// When the user calls SetDlgItemUrl, then the static-control-subclass is added
// if it wasn't already there, and the dialog-subclass is added if it wasn't
// already there. Both subclasses are removed in response to their respective
// WM_DESTROY messages. Also, each subclass stores a property in its window,
// which is a HGLOBAL handle to a GlobalAlloc'd structure:
typedef struct {TCHAR *url; WNDPROC oldproc; HFONT hf; HBRUSH hb;} TUrlData;
// I'm a miser and only defined a single structure, which is used by both
// the control-subclass and the dialog-subclass. Both of them use 'oldproc' of course.
// The control-subclass only uses 'url' (in response to WM_LBUTTONDOWN),
// and the dialog-subclass only uses 'hf' and 'hb' (in response to WM_CTLCOLORSTATIC)
// There is one sneaky thing to note. We create our underlined font *lazily*.
// Initially, hf is just NULL. But the first time the subclassed dialog received
// WM_CTLCOLORSTATIC, we sneak a peak at the font that was going to be used for
// the control, and we create our own copy of it but including the underline style.
// This way our code works fine on dialogs of any font.

// SetDlgItemUrl: this is the routine that sets up the subclassing.
void SetDlgItemUrl(HWND hdlg,int id,const TCHAR *url) 
{ // nb. vc7 has crummy warnings about 32/64bit. My code's perfect! That's why I hide the warnings.
  #pragma warning( push )
  #pragma warning( disable: 4312 4244 )
  // First we'll subclass the edit control
  HWND hctl = GetDlgItem(hdlg,id);
  SetWindowText(hctl,url);
  HGLOBAL hold = (HGLOBAL)GetProp(hctl,_T("href_dat"));
  if (hold!=NULL) // if it had been subclassed before, we merely need to tell it the new url
  { TUrlData *ud = (TUrlData*)GlobalLock(hold);
    delete[] ud->url;
    ud->url=new TCHAR[_tcslen(url)+1]; _tcscpy(ud->url,url);
  }
  else
  { HGLOBAL hglob = GlobalAlloc(GMEM_MOVEABLE,sizeof(TUrlData));
    TUrlData *ud = (TUrlData*)GlobalLock(hglob);
    ud->oldproc = (WNDPROC)GetWindowLongPtr(hctl,GWLP_WNDPROC);
    ud->url=new TCHAR[_tcslen(url)+1]; _tcscpy(ud->url,url);
    ud->hf=0; ud->hb=0;
    GlobalUnlock(hglob);
    SetProp(hctl,_T("href_dat"),hglob);
    SetWindowLongPtr(hctl,GWLP_WNDPROC,(LONG_PTR)UrlCtlProc);
  }
  //
  // Second we subclass the dialog
  hold = (HGLOBAL)GetProp(hdlg,_T("href_dlg"));
  if (hold==NULL)
  { HGLOBAL hglob = GlobalAlloc(GMEM_MOVEABLE,sizeof(TUrlData));
    TUrlData *ud = (TUrlData*)GlobalLock(hglob);
    ud->url=0;
    ud->oldproc = (WNDPROC)GetWindowLongPtr(hdlg,GWLP_WNDPROC);
    ud->hb=CreateSolidBrush(GetSysColor(COLOR_BTNFACE));
    ud->hf=0; // the font will be created lazilly, the first time WM_CTLCOLORSTATIC gets called
    GlobalUnlock(hglob);
    SetProp(hdlg,_T("href_dlg"),hglob);
    SetWindowLongPtr(hdlg,GWLP_WNDPROC,(LONG_PTR)UrlDlgProc);
  }
  #pragma warning( pop )
}

// UrlCtlProc: this is the subclass procedure for the static control
LRESULT CALLBACK UrlCtlProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ HGLOBAL hglob = (HGLOBAL)GetProp(hwnd,_T("href_dat"));
  if (hglob==NULL) return DefWindowProc(hwnd,msg,wParam,lParam);
  TUrlData *oud=(TUrlData*)GlobalLock(hglob); TUrlData ud=*oud;
  GlobalUnlock(hglob); // I made a copy of the structure just so I could GlobalUnlock it now, to be more local in my code
  switch (msg)
  { case WM_DESTROY:
    { RemoveProp(hwnd,_T("href_dat")); GlobalFree(hglob);
      if (ud.url!=0) delete[] ud.url;
      // nb. remember that ud.url is just a pointer to a memory block. It might look weird
      // for us to delete ud.url instead of oud->url, but they're both equivalent.
    } break;
    case WM_LBUTTONDOWN:
    { HWND hdlg=GetParent(hwnd); if (hdlg==0) hdlg=hwnd;
      ShellExecute(hdlg,_T("open"),ud.url,NULL,NULL,SW_SHOWNORMAL);
    } break;
    case WM_SETCURSOR:
    { SetCursor(LoadCursor(NULL,MAKEINTRESOURCE(IDC_HAND)));
      return TRUE;
    } 
    case WM_NCHITTEST:
    { return HTCLIENT; // because normally a static returns HTTRANSPARENT, so disabling WM_SETCURSOR
    } 
  }
  return CallWindowProc(ud.oldproc,hwnd,msg,wParam,lParam);
}
  
// UrlDlgProc: this is the subclass procedure for the dialog
LRESULT CALLBACK UrlDlgProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ HGLOBAL hglob = (HGLOBAL)GetProp(hwnd,_T("href_dlg"));
  if (hglob==NULL) return DefWindowProc(hwnd,msg,wParam,lParam);
  TUrlData *oud=(TUrlData*)GlobalLock(hglob); TUrlData ud=*oud;
  GlobalUnlock(hglob);
  switch (msg)
  { case WM_DESTROY:
    { RemoveProp(hwnd,_T("href_dlg")); GlobalFree(hglob);
      if (ud.hb!=0) DeleteObject(ud.hb);
      if (ud.hf!=0) DeleteObject(ud.hf);
    } break;
    case WM_CTLCOLORSTATIC:
    { HDC hdc=(HDC)wParam; HWND hctl=(HWND)lParam;
      // To check whether to handle this control, we look for its subclassed property!
      HANDLE hprop=GetProp(hctl,_T("href_dat")); if (hprop==NULL) return CallWindowProc(ud.oldproc,hwnd,msg,wParam,lParam);
      // There has been a lot of faulty discussion in the newsgroups about how to change
      // the text colour of a static control. Lots of people mess around with the
      // TRANSPARENT text mode. That is incorrect. The correct solution is here:
      // (1) Leave the text opaque. This will allow us to re-SetDlgItemText without it looking wrong.
      // (2) SetBkColor. This background colour will be used underneath each character cell.
      // (3) return HBRUSH. This background colour will be used where there's no text.
      SetTextColor(hdc,RGB(0,0,255));
      SetBkColor(hdc,GetSysColor(COLOR_BTNFACE));
      if (ud.hf==0)
      { // we use lazy creation of the font. That's so we can see font was currently being used.
        TEXTMETRIC tm; GetTextMetrics(hdc,&tm);
        LOGFONT lf;
        lf.lfHeight=tm.tmHeight;
        lf.lfWidth=0;
        lf.lfEscapement=0;
        lf.lfOrientation=0;
        lf.lfWeight=tm.tmWeight;
        lf.lfItalic=tm.tmItalic;
        lf.lfUnderline=TRUE;
        lf.lfStrikeOut=tm.tmStruckOut;
        lf.lfCharSet=tm.tmCharSet;
        lf.lfOutPrecision=OUT_DEFAULT_PRECIS;
        lf.lfClipPrecision=CLIP_DEFAULT_PRECIS;
        lf.lfQuality=DEFAULT_QUALITY;
        lf.lfPitchAndFamily=tm.tmPitchAndFamily;
        GetTextFace(hdc,LF_FACESIZE,lf.lfFaceName);
        ud.hf=CreateFontIndirect(&lf);
        TUrlData *oud = (TUrlData*)GlobalLock(hglob); oud->hf=ud.hf; GlobalUnlock(hglob);
      }
      SelectObject(hdc,ud.hf);
      // Note: the win32 docs say to return an HBRUSH, typecast as a BOOL. But they
      // fail to explain how this will work in 64bit windows where an HBRUSH is 64bit.
      // I have supressed the warnings for now, because I hate them...
      #pragma warning( push )
      #pragma warning( disable: 4311 )
      return (BOOL)ud.hb;
      #pragma warning( pop )
    }  
  }
  return CallWindowProc(ud.oldproc,hwnd,msg,wParam,lParam);
}



inline void Debug(const tstring s)
{ if (DebugFile==_T("")) return;
  if (DebugFile==_T("OutputDebugString")) {tstring err=s+_T("\r\n"); OutputDebugString(err.c_str());}
  else {FILE *f = _tfopen(DebugFile.c_str(),_T("a+t")); _ftprintf(f,_T("%s\n"),s.c_str()); fclose(f);}
}



void RegSave(const tstring name,DWORD type,void*buf,int size)
{ tstring path = _T("Software\\Scrplus\\")+SaverName;
  HKEY skey; LONG res=RegCreateKeyEx(HKEY_CURRENT_USER,path.c_str(),0,0,0,KEY_ALL_ACCESS,0,&skey,0);
  if (res!=ERROR_SUCCESS) return;
  RegSetValueEx(skey,name.c_str(),0,type,(LPCBYTE)buf,size);
  RegCloseKey(skey);
}
bool RegLoadDword(const tstring name,DWORD *buf)
{ tstring path = _T("Software\\Scrplus\\")+SaverName;
  HKEY skey; LONG res=RegOpenKeyEx(HKEY_CURRENT_USER,path.c_str(),0,KEY_READ,&skey);
  if (res!=ERROR_SUCCESS) return false;
  DWORD size=sizeof(DWORD);
  res=RegQueryValueEx(skey,name.c_str(),0,0,(LPBYTE)buf,&size);
  RegCloseKey(skey);
  return (res==ERROR_SUCCESS);
}

void RegSave(const tstring name,int val)
{ DWORD v=val; RegSave(name,REG_DWORD,&v,sizeof(v));
}
void RegSave(const tstring name,bool val)
{ RegSave(name,val?1:0);
}
void RegSave(const tstring name,tstring val)
{ RegSave(name,REG_SZ,(void*)val.c_str(),sizeof(TCHAR)*(val.length()+1));
}
int RegLoad(const tstring name,int def)
{ DWORD val; bool res=RegLoadDword(name,&val);
  return res?val:def;
}
bool RegLoad(const tstring name,bool def)
{ int b=RegLoad(name,def?1:0); return (b!=0);
}
tstring RegLoad(const tstring name,tstring def)
{ tstring path = _T("Software\\Scrplus\\")+SaverName;
  HKEY skey; LONG res=RegOpenKeyEx(HKEY_CURRENT_USER,path.c_str(),0,KEY_READ,&skey);
  if (res!=ERROR_SUCCESS) return def;
  DWORD size=0; res=RegQueryValueEx(skey,name.c_str(),0,0,0,&size);
  if (res!=ERROR_SUCCESS) {RegCloseKey(skey); return def;}
  TCHAR *buf = new TCHAR[size];
  RegQueryValueEx(skey,name.c_str(),0,0,(LPBYTE)buf,&size);
  tstring s(buf); delete[] buf;
  RegCloseKey(skey);
  return s;
}



int WINAPI WinMain(HINSTANCE h,HINSTANCE,LPSTR,int)
{ hInstance=h;
  TCHAR name[MAX_PATH]; int sres=LoadString(hInstance,1,name,MAX_PATH);
  if (sres==0) {MessageBox(NULL,_T("Must store saver name as String Resource 1"),_T("Saver"),MB_ICONERROR|MB_OK);return 0;}
  SaverName=name;
  //
  TCHAR mod[MAX_PATH]; GetModuleFileName(hInstance,mod,MAX_PATH); tstring smod(mod);
  bool isexe = (smod.find(_T(".exe"))!=tstring::npos || smod.find(_T(".EXE"))!=tstring::npos);
  //
  TCHAR *c=GetCommandLine();
  if (*c=='\"') {c++; while (*c!=0 && *c!='\"') c++; if (*c=='\"') c++;} else {while (*c!=0 && *c!=' ') c++;}
  while (*c==' ') c++;
  HWND hwnd=NULL; bool fakemulti=false;
  if (*c==0) {if (isexe) ScrMode=smInstall; else ScrMode=smConfig; hwnd=NULL;}
  else
  { if (*c=='-' || *c=='/') c++;
    if (*c=='u' || *c=='U') ScrMode=smUninstall;
    if (*c=='p' || *c=='P' || *c=='l' || *c=='L')
    { c++; while (*c==' ' || *c==':') c++;
      if (_tcsicmp(c,_T("scrprev"))==0) hwnd=CheckForScrprev(); else hwnd=(HWND)_ttoi(c); 
      ScrMode=smPreview;
    }
    else if (*c=='s' || *c=='S') {ScrMode=smSaver; fakemulti=(c[1]=='m'||c[1]=='M');}
    else if (*c=='c' || *c=='C') {c++; while (*c==' ' || *c==':') c++; if (*c==0) hwnd=GetForegroundWindow(); else hwnd=(HWND)_ttoi(c); ScrMode=smConfig;}
    else if (*c=='a' || *c=='A') {c++; while (*c==' ' || *c==':') c++; hwnd=(HWND)_ttoi(c); ScrMode=smPassword;}
  }
  //
  if (ScrMode==smInstall) {DoInstall(); return 0;}
  if (ScrMode==smUninstall) {DoUninstall(); return 0;}
  if (ScrMode==smPassword) {ChangePassword(hwnd); return 0;}
  //
  ReadGeneralRegistry();
  //
  INITCOMMONCONTROLSEX icx; ZeroMemory(&icx,sizeof(icx));
  icx.dwSize=sizeof(icx);
  icx.dwICC=ICC_UPDOWN_CLASS;
  InitCommonControlsEx(&icx);
  //
  WNDCLASS wc; ZeroMemory(&wc,sizeof(wc));
  wc.hInstance=hInstance;
  wc.hCursor=LoadCursor(NULL,IDC_ARROW);
  wc.lpszClassName=_T("ScrMonitor");
  wc.lpfnWndProc=MonitorWndProc;
  RegisterClass(&wc);
  //
  wc.lpfnWndProc=SaverWndProc;
  wc.cbWndExtra=8;
  wc.lpszClassName=_T("ScrClass");
  RegisterClass(&wc);
  //
  wc.lpfnWndProc=WaveformWndProc;
  wc.cbWndExtra=0;
  wc.hbrBackground=(HBRUSH)GetStockObject(BLACK_BRUSH);
  wc.lpszClassName=_T("Waveform");
  RegisterClass(&wc);
  //

  //
  if (ScrMode==smConfig) DoConfig(hwnd);
  else if (ScrMode==smSaver || ScrMode==smPreview) DoSaver(hwnd,fakemulti);
  //
  return 0;
}
