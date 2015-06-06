#define COMPILE_MULTIMON_STUBS
#include <windows.h>
#include <multimon.h>
#include <mmsystem.h>
#include <stdlib.h>
#include <stdio.h>
#include <math.h>
#include <string>
#include <vector>
#include <list>
using namespace std;
#include "wmp.h"
#include "../body.h"
#include "../utils.h"
using namespace stk;


const CLSID StickyCLSID = {0x20B2D629,0x386F,0x4e59,0xB3,0x4D,0x4C,0xF7,0x3D,0x74,0xB1,0xCF};
HINSTANCE hInstance;
long gref=0;  // global reference count for objects in this DLL for the unload test

list<string> recents;  // the recent music we have heard


    





// -----------------------------------------------------------------------------
// TStickyViz. This is the actual viz that is implemented by this file.
// -----------------------------------------------------------------------------
//
class TStickyViz : public IWMPEffects2
{
public:
  long ref;     // Reference counting for IUnknown
  HBITMAP bmp;  // We will use double-buffering to reduce flicker
  int bx,by;    // The size of our double-buffering bitmap
  HWND hfull;   // When we're fullscreen we use a window we create ourselves
  HWND hparent; // WMP9 will call our Create method and tell us to use this hparent
  bool MyRight; // I (the plugin) will subclass+steal the right-click menu
  HANDLE hevent;// when the Options dialog closes, it sets this global event
  unsigned long nexteventtime; // and we check the state of the event periodically
public:
  // IUnknown...
  HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void **ppv) {if (riid==IID_IUnknown || riid==IID_IWMPEffects || riid==IID_IWMPEffects2) {*ppv=this; AddRef(); return S_OK;} else return E_NOINTERFACE;}
  ULONG STDMETHODCALLTYPE AddRef() {InterlockedIncrement(&gref); return InterlockedIncrement(&ref);}
  ULONG STDMETHODCALLTYPE Release() {int tmp = InterlockedDecrement(&ref); if (tmp==0) delete this; InterlockedDecrement(&gref); return tmp;}
  // IWMPEffects...
  HRESULT STDMETHODCALLTYPE Render(TimedLevel *pLevels, HDC hdc, RECT *rc);
  HRESULT STDMETHODCALLTYPE MediaInfo(LONG lChannelCount, LONG lSampleRate, BSTR bstrTitle);
  HRESULT STDMETHODCALLTYPE GetCapabilities(DWORD * pdwCapabilities);
  HRESULT STDMETHODCALLTYPE GoFullscreen(BOOL fFullScreen);
  HRESULT STDMETHODCALLTYPE RenderFullScreen(TimedLevel *pLevels);
  HRESULT STDMETHODCALLTYPE DisplayPropertyPage(HWND hwndOwner);
  HRESULT STDMETHODCALLTYPE GetTitle(BSTR *bstrTitle);
  HRESULT STDMETHODCALLTYPE GetPresetTitle(LONG nPreset, BSTR *bstrPresetTitle);
  HRESULT STDMETHODCALLTYPE GetPresetCount(LONG *pnPresetCount);
  HRESULT STDMETHODCALLTYPE SetCurrentPreset(LONG nPreset);
  HRESULT STDMETHODCALLTYPE GetCurrentPreset(LONG *pnPreset);
  // IWMPEffects2...
  virtual HRESULT STDMETHODCALLTYPE SetCore(IWMPCore *pPlayer);
  HRESULT STDMETHODCALLTYPE Create(HWND hwndParent);
  HRESULT STDMETHODCALLTYPE Destroy();
  HRESULT STDMETHODCALLTYPE NotifyNewMedia(IWMPMedia *pMedia);
  HRESULT STDMETHODCALLTYPE OnWindowMessage(UINT msg,WPARAM WParam,LPARAM LParam,LRESULT *plResultParam);
  HRESULT STDMETHODCALLTYPE RenderWindowed(TimedLevel *pData,BOOL fRequiredRender);
  // TStickyViz...
  TStickyViz() : ref(0), preset(0), bmp(0), hfull(0), hparent(0), misnew(false), hevent(0), nexteventtime(0)
  { StickyInit(); srand(GetTickCount());
    hevent=CreateEvent(NULL,FALSE,FALSE,"Lu:Sticky:OptionsChanged");
  }
  ~TStickyViz() {StickyEnd(); if (hfull!=0) DestroyWindow(hfull); hfull=0; if (bmp!=0) DeleteObject(bmp); bmp=0; if (hevent!=0) CloseHandle(hevent); hevent=0;}
  void PropSheetOkay(bool mr) {MyRight=mr; RegSave(); SetEvent(hevent);}
  void EnsureDirectoryUpToDate()
  { DWORD res = WaitForSingleObject(hdir,0);
    if (res==WAIT_OBJECT_0)
    { ReadDirectory();
      FindNextChangeNotification(hdir);
    }
  }
  void OnRightClick(HWND hwnd,int x,int y);
  //
  // Sticky behaviour
  void StickyInit();      // sets up timings, "body" pointer, &c.
  void StickyEnd();
  void NewMedia(const string artist, const string title, const string filename);
  void RegLoad();         // sticky stores its colour+timing+other preferences
  void RegSave();         // in the registry
  void SelectBody(bool showrand=false); string curboddesc0;     // loads the body based on the "preset" variable
  void Calc(TimedLevel *pLevels,DWORD nowtime); // updates cmul,l[],r[],k[] arrays
  void UpdateBody();      // given cmul,l[],r[],k[] arrays, recalculates limb positions
  void ReadDirectory();   // updates the internal list according to the contents of the stick directory
  // sticky properties
  string martist, mtitle, mfilename, curregexp; bool misnew; // set by MediaInfo/NotifyNewMedia
  list<TRule> rules;      // rules for selecting which stick to show
  int showAmp;            // should we show amplitude sticks?
  int preset;             // index of what was selected by the user, so preset0=Randomization
  vector<TPre> pre;       // all of the presets: their display-names, and full paths. Populated in finalconstruct.
  string banner; unsigned int bantime; // a text banner, plus when we stop displaying it.
  double l[6],r[6],k[6];  // we divide the spectrum up into 6 bands, plus k for karaoke
  double cmul;            // this is the time since the last update
  bool needsredraw;       // a tick from WMP merely calls Calc,needsredraw and calls InvalidateRect.
  double max[6];          // record the maximum intensity we've received (by frequency)
  double min[6];          // and the maximum
  double kmin[6],kmax[6]; // same for karaoke
  bool GotSoundYet;       // have we yet got any sound?
  DWORD prevtime;         // CPU tick count of the previous calculuation
  DWORD calctime;         // CPU tick count for when we should next recalculate limb positions
  int period[10],periodi; // for fps counter
  DWORD periodtime;       // for fps counter, the CPU tick count of the previous frame
  TBody *body;            // the stick figure himself!
  HANDLE hdir;            // notifications when the stick directory changes
};

TStickyViz *debugviz=0;
void stk::luassertmsg(const char *s,const char *msg,const char *f,unsigned int l)
{ if (debugviz==0) return;
  char c[2000]; wsprintf(c,"%s '%s' - %s:%u",msg,s,f,l);
  debugviz->banner=c; debugviz->bantime=GetTickCount()+3000;
}



void TStickyViz::StickyInit()
{ if (debugviz==0) debugviz=this;
  preset = 0;
  showAmp = 0;
  MyRight = true;
  bantime=0; curboddesc0=""; curregexp="";
  prevtime=0; calctime=0; cmul=0;
  ZeroMemory(period,sizeof(period)); periodi=0; periodtime=0;
  body=new TBody();
  banner="";
  l[0]=0; r[0]=0; max[0]=2600; min[0]=150; 
  l[1]=0; r[1]=0; max[1]=2300; min[1]=140; 
  l[2]=0; r[2]=0; max[2]=2100; min[2]=130; 
  l[3]=0; r[3]=0; max[3]=1900; min[3]=120;    
  l[4]=0; r[4]=0; max[4]=1700; min[4]=110;    
  l[5]=0; r[5]=0; max[5]=1500; min[5]=100;
  k[0]=0; kmax[0]=400; kmin[0]=20;
  k[1]=0; kmax[1]=400; kmin[1]=20;
  needsredraw=true;
  GotSoundYet=false;
  hdir = FindFirstChangeNotification(GetStickDir().c_str(),TRUE,FILE_NOTIFY_CHANGE_FILE_NAME|FILE_NOTIFY_CHANGE_DIR_NAME|FILE_NOTIFY_CHANGE_SIZE|FILE_NOTIFY_CHANGE_LAST_WRITE);

  load_rules(&rules);
  ReadDirectory();
  for (int i=0; i<(int)pre.size(); i++)
  { string s = pre[i].desc;
    if (StringLower(s).find("justin")!=string::npos) preset=i;
  }

  RegLoad();
  if (preset>(int)pre.size()) preset=0;
  NewMedia("","","");
}

void TStickyViz::StickyEnd()
{ if (body!=0) delete body; body=0;
  save_recents(&recents);
  if (debugviz==this) debugviz=0;
}


void TStickyViz::ReadDirectory()
{ pre.clear();
  // first we add any files
  DirScan(pre,GetStickDir()+"\\");
  HideRedundancy(pre);
  // next we add resources if they're not already there
  if (pre.size()==0)
  { for (unsigned int i=1; ; i++)
    { char c[MAX_PATH]; int res=LoadString(hInstance,i+1000,c,MAX_PATH);
      if (res==0) break;
      string desc=c, ldesc=StringLower(desc); wsprintf(c,"*%i",i+1000); string path=c;
      vector<TPre>::iterator pi=pre.begin();
      // we'll see if our resources was present anywhere in the list. That
      // way, if the resource was present in a subdirectory, it will still count.
      bool wasalreadythere=false;
      for (unsigned int i=0; i<pre.size(); i++)
      { string pd = StringLower(pre[i].desc);
        if (strstr(pd.c_str(), ldesc.c_str())!=0) wasalreadythere=true;
      }
      if (!wasalreadythere)
      { vector<TPre>::iterator di=pre.begin();
        while (di!=pre.end() && stricmp(di->desc.c_str(),desc.c_str())<0) di++;
        TPre p; p.path=path; p.desc=desc; pre.insert(di,p);
      }
    }
  }
  if (preset > (int)pre.size()) preset=0;
}


bool OpenFile(TBody **b,string fn,char *err)
{ if (fn=="") return false;
  if (fn[0]!='*') return LoadBody(b,fn.c_str(),err,lbForUse);
  int id=atoi(fn.substr(1).c_str());
  HRSRC hrsrc = FindResource(hInstance,MAKEINTRESOURCE(id),RT_RCDATA);
  DWORD size = SizeofResource(hInstance,hrsrc);
  HGLOBAL hglob = LoadResource(hInstance,hrsrc);
  char *rs = (char*)LockResource(hglob);
  // NB. The ReadData call is destructive. Therefore we must take
  // a copy of the memory block.
  char *buf = new char[size+1];
  memcpy(buf,rs,size); buf[size]=0;
  bool res=(*b)->ReadData(buf,err,rdOverwrite,NULL);
  delete[] buf;
  return res;
}


void TStickyViz::SelectBody(bool showrand)
{ int i;
  if (preset!=0) i=preset-1;
  else i=ChooseRandomBody(curregexp,pre,curboddesc0);
  curboddesc0 = pre[i].desc;
  char err[1000];
  bool res=OpenFile(&body,pre[i].path.c_str(),err);
  if (!res) banner=err;
  else if (hfull==0) banner="";
  else banner=ExtractFileName(pre[i].desc);
  if (res && showrand && preset==0 && !hfull) banner="Randomize";
  bantime=GetTickCount()+3000;
}



void TStickyViz::Calc(TimedLevel *pLevels, DWORD nowtime)
{ cmul=0;
  if (pLevels!=0)
  { bool adjust=false;
    if (pLevels->state==play_state)
    { cmul = ((double)(nowtime-prevtime))/10.0; // diff as a multiple of 10ms, the standard interval
      // The max-ratings will fade over time
      if (GotSoundYet)
      { if (cmul>10) // we fade no more frequently than ten times a second.
        { for (int p=0; p<6; p++)
          { max[p]*=0.999; kmax[p]*=0.997;
            if (min[p]<22) min[p]=22; min[p]*=1.002;
            if (kmin[p]<12) kmin[p]=12; kmin[p]*=1.002;
          }
          adjust=true;
          prevtime=nowtime;
        }
      }
      //
      // the frequency[] array is 1024 elements big, first element 20Hz, last one 22050Hz.
      // So our bands are, in hertz
      // 0:  450 -  772
      // 1:  794 - 1117
      // 2: 1134 - 1461
      // 3: 1480 - 1827
      // 4: 1827 - 2171
      // 5: 2171 - 2515
      int boff=20, bwidth=16;
      for (int p=0; p<6; p++) 
      { l[p]=0; r[p]=0; int i;
        for (i=0; i<bwidth; i++)
        { l[p] += pLevels->frequency[0][boff+p*bwidth+i];
          r[p] += pLevels->frequency[1][boff+p*bwidth+i];
        }
        if (l[p]>max[p]) max[p]=l[p];
        if (r[p]>max[p]) max[p]=r[p];
        if (adjust)
        { if (l[p]<min[p] || r[p]<min[p]) min[p]*=0.9;
        }
      }
      //
      // the waveform is also 1024 elements big, stored as unsigned chars.
      // 0.vocals=diff, 1.music=average
      int kvoc=0, kmus=0;
      for (int i=0; i<1024; i++)
      { int voc = (pLevels->waveform[0][i]+pLevels->waveform[1][i])/2-128;
        int mus = (int)(pLevels->waveform[0][i])-(int)(pLevels->waveform[1][i]);
        kvoc += voc*voc; kmus += mus*mus;
      }
      k[0]=(int)sqrt(kvoc); k[1]=2*(int)sqrt(kmus);
      if (k[0]<kmin[0]) kmin[0]=k[0];  if (k[0]>kmax[0]) kmax[0]=k[0];
      if (k[1]<kmin[1]) kmin[1]=k[1];  if (k[1]>kmax[1]) kmax[1]=k[1];
    }
    for (int p=0; p<6; p++)
    { l[p] = (l[p]-min[p]) / (max[p]-min[p]+1); if (l[p]<0) l[p]=0; l[p]*=l[p];
      r[p] = (r[p]-min[p]) / (max[p]-min[p]+1); if (r[p]<0) r[p]=0; r[p]*=r[p];
    }
    for (int p=0; p<2; p++)
    { k[p] = (k[p]-kmin[p]) / (kmax[p]-kmin[p]+1); if (k[p]<0) k[p]=0; k[p]*=k[p];
    }
    for (int p=0; p<6; p++)
    { if (l[p]>0.01 || r[p]>0.01) {if (!GotSoundYet) prevtime=nowtime; GotSoundYet=true;}
    }
    //dlog(l,r);
  }
  if (pLevels==0 || !GotSoundYet)
  { prevtime=0; for (int p=0; p<6; p++) {l[p]=0.2; r[p]=0.2; k[p]=0.2;}
  }
}


// given l[] and r[] and k[], this function just recalculates the bodies
void TStickyViz::UpdateBody()
{ double freq[3][6];
  for (int p=0; p<6; p++) {freq[0][p]=l[p]; freq[1][p]=r[p]; freq[2][p]=k[p];}
  body->AssignFreq(freq,cmul);
  body->RecalcEffects();
  body->Recalc();
}














void TStickyViz::RegLoad()
{ HKEY hkey;
  LONG res = RegOpenKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,KEY_READ,&hkey);
  if (res!=ERROR_SUCCESS) return;
  char uf[200],sa[200]; *uf=0;*sa=0; DWORD type,size, ddat=0;
  size=200; res=RegQueryValueEx(hkey,"showAmp",0,&type,(LPBYTE)sa,&size); if (res!=ERROR_SUCCESS) *sa=0;
  size=sizeof(ddat); res=RegQueryValueEx(hkey,"StealRightClickEx",0,&type,(LPBYTE)&ddat,&size); if (res!=ERROR_SUCCESS) ddat=1;
  RegCloseKey(hkey);
  int i, ires=sscanf(sa,"%i",&i); if (ires==1) showAmp = i;
  MyRight = (ddat!=0);
}

void TStickyViz::RegSave()
{ HKEY hkey; DWORD disp;
  LONG res = RegCreateKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,NULL,0,KEY_WRITE,NULL,&hkey,&disp);
  if (res!=ERROR_SUCCESS) return;
  char sa[200];
  wsprintf(sa,"%i",(int)showAmp);
  RegSetValueEx(hkey,"showAmp",0,REG_SZ,(LPBYTE)sa,(DWORD)strlen(sa)+1);
  DWORD ddat=(MyRight?1:0); RegSetValueEx(hkey,"StealRightClickEx",0,REG_DWORD,(LPBYTE)&ddat,sizeof(ddat));
  RegCloseKey(hkey);
  //
  // We also save our version of the preset. That's because merely selecting a preset from our
  // own right-click menu is not enough to make WMP remember the change to disk.
  // NEWS-FLASH:
  // This doesn't work. Why? Because WMP remembers to change its previous version on disk.
  /*
  res = RegCreateKeyEx(HKEY_CURRENT_USER,"Software\\Microsoft\\MediaPlayer\\Preferences",0,NULL,0,KEY_READ|KEY_WRITE,NULL,&hkey,&disp);
  if (res!=ERROR_SUCCESS) return;
  char pname[200]; DWORD size=200; DWORD type;
  res=RegQueryValueEx(hkey,"CurrentEffectType",0,&type,(LPBYTE)pname,&size);
  if (res==ERROR_SUCCESS && strcmp(pname,"Sticky")==0)
  { ddat=preset; RegSetValueEx(hkey,"CurrentEffectPreset",0,REG_DWORD,(LPBYTE)&ddat,sizeof(ddat));
  }
  RegCloseKey(hkey);
  */
}





// Render: called each time there's new data. NB. We MUST draw at
// this time, otherwise the display will flicker when a skin is applied
STDMETHODIMP TStickyViz::Render(TimedLevel *pLevels, HDC hwdc, RECT *prc)
{ DWORD tcnowtime=GetTickCount(); 
  //
  // check if any changes have come our way from the property sheet
  if (tcnowtime>nexteventtime)
  { DWORD wres = WaitForSingleObject(hevent,0);
    if (wres==WAIT_OBJECT_0) RegLoad();
    nexteventtime = tcnowtime+2000;
  }
  
  //
  // lazy updating of media info. (see comments in MediaInfo method)
  if (misnew) {NewMedia(martist,mtitle,mfilename); misnew=false;}
  
  //
  if (banner!="" && tcnowtime>bantime) banner="";
  if (pLevels->state==stop_state || pLevels->state==pause_state) GotSoundYet=false;
  if (calctime!=0 && tcnowtime<calctime) {} // no need to recalculate
  else
  { calctime = tcnowtime + 1000/body->fps;
    Calc(pLevels,tcnowtime); needsredraw=true;
  }

  DWORD this_period=tcnowtime-periodtime;
  period[periodi]=this_period;
  periodi++; if (periodi==10) periodi=0;
  periodtime=tcnowtime;

  DWORD nowtime=0,aftercalc=0,afterdraw=0,afterblt=0; TIMECAPS tc; tc.wPeriodMin=1;
  if (showAmp) {timeGetDevCaps(&tc,sizeof(TIMECAPS));timeBeginPeriod(tc.wPeriodMin);}

  nowtime=timeGetTime();
  if (needsredraw) UpdateBody(); needsredraw=false;
  aftercalc=timeGetTime();

  RECT &rc=*prc;
  int width=rc.right-rc.left, height=rc.bottom-rc.top;
  // We will use double-buffering
  HDC hdc=CreateCompatibleDC(hwdc);
  if (bmp!=0 && (width!=bx || height!=by)) {DeleteObject(bmp); bmp=0;}
  if (bmp==0) {bmp=CreateCompatibleBitmap(hwdc,width,height); bx=width; by=height;}
  HGDIOBJ holdbm = SelectObject(hdc,bmp);

  TAmpData ad; ad.l=l; ad.r=r; ad.min=min; ad.max=max; ad.k=k; ad.kmin=kmin; ad.kmax=kmax;
  string err;
  bool ok=SimpleDraw(hdc,rc,body,banner.c_str(),showAmp?&ad:0,&err);
  if (!ok) {banner=err; bantime=nowtime+5000;}

  afterdraw=timeGetTime();

  BitBlt(hwdc,rc.left,rc.top,width,height,hdc,0,0,SRCCOPY);
  SelectObject(hdc,holdbm);
  DeleteDC(hdc);
  afterblt=timeGetTime();
  if (showAmp)
  { int avperiod=0; for (int i=0; i<10; i++) avperiod+=period[i]; avperiod/=10;
    int fps=0; if (avperiod>0) fps=1000/avperiod;
    banner= "Calc:"+StringInt(aftercalc-nowtime)+"   "
            "Draw:"+StringInt(afterdraw-aftercalc)+"   "
            "Blt:"+StringInt(afterblt-afterdraw)+"  "
            "FPS:"+StringInt(fps);
    if (hfull!=0) banner="(full) "+banner;
    bantime=nowtime+3000;
  }
  if (showAmp) timeEndPeriod(tc.wPeriodMin);

  
  return S_OK;
}



LRESULT CALLBACK FullWndProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ switch (msg)
  { case WM_CREATE:
    { CREATESTRUCT *cs=(CREATESTRUCT*)lParam;
      SetWindowLongPtr(hwnd,GWLP_USERDATA,(LONG)(LONG_PTR)cs->lpCreateParams);
    } break;
    case WM_LBUTTONDOWN: case WM_RBUTTONUP: case WM_KEYDOWN:
    { HWND *hfull = (HWND*)(LONG_PTR)GetWindowLongPtr(hwnd,GWLP_USERDATA);
      *hfull=0; DestroyWindow(hwnd);
      // hfull is a pointer to the 'hfull' member variable within the TStickyViz instance.
      // By setting it to zero, this is how we will inform the instance (and thence
      // wmp itself) to jump out of fullscreen mode.
      // Note: we respond to rightbuttonup instead of rightbuttondown, because if we
      // responded to down, then the desktop/wmp underneath would get the subsequent
      // buttonup and display a popup menu or something.
    } break;
    case WM_SYSCOMMAND:
    { if (wParam==SC_SCREENSAVE) return 0; // gobble it up! so disabling the screensaver
    } break;
    case WM_ERASEBKGND: return TRUE;
    case WM_SETCURSOR: SetCursor(NULL); return TRUE;
    case WM_PAINT: {PAINTSTRUCT ps; BeginPaint(hwnd,&ps); EndPaint(hwnd,&ps);} return TRUE;
  }
  return DefWindowProc(hwnd,msg,wParam,lParam);
}


// GoFullScreen: it's possible to change screen resolution here, if you want.
STDMETHODIMP TStickyViz::GoFullscreen(BOOL fFullScreen)
{ if (fFullScreen && hfull==0)
  { WNDCLASSEX wcex; ZeroMemory(&wcex,sizeof(wcex)); wcex.cbSize = sizeof(WNDCLASSEX);
    BOOL res=GetClassInfoEx(hInstance,"WmpVizFullClass",&wcex);
    if (!res)
    { wcex.style = CS_HREDRAW | CS_VREDRAW;
      wcex.lpfnWndProc = (WNDPROC)FullWndProc;
      wcex.hInstance = hInstance;
      wcex.lpszClassName = "WmpVizFullClass";
      ATOM res=RegisterClassEx(&wcex);
      if (res==0) return E_FAIL;
    }
    #ifndef SM_XVIRTUALSCREEN
    #define SM_XVIRTUALSCREEN       76
    #define SM_YVIRTUALSCREEN       77
    #define SM_CXVIRTUALSCREEN      78
    #define SM_CYVIRTUALSCREEN      79
    #endif
    int x=GetSystemMetrics(SM_YVIRTUALSCREEN), y=GetSystemMetrics(SM_XVIRTUALSCREEN);
    int w=GetSystemMetrics(SM_CXVIRTUALSCREEN), h=GetSystemMetrics(SM_CYVIRTUALSCREEN);
    hfull=CreateWindowEx(WS_EX_TOPMOST,"WmpVizFullClass","WmpVizFull",WS_POPUP|WS_VISIBLE,x,y,w,h,NULL,NULL,hInstance,&hfull);
  }
  else
  { if (hfull!=0) DestroyWindow(hfull); hfull=0;
  }
  return S_OK;
}

STDMETHODIMP TStickyViz::RenderFullScreen(TimedLevel *pLevels)
{ // if full-screen is over, we tell wmp that it's over through the following response:
  if (hfull==0) return E_FAIL;
  // Otherwise, just call the regular 'Render' function...
  RECT rc; GetClientRect(hfull,&rc);
  HDC hdc=GetDC(hfull);
  Render(pLevels,hdc,&rc);
  ReleaseDC(hfull,hdc);
  return S_OK;
}

// RenderWindowed: called by WMP9 if it created us in window mode rather than windowless mode
STDMETHODIMP TStickyViz::RenderWindowed(TimedLevel *pData,BOOL fRequiredRender)
{ fRequiredRender; // unused
  HDC hdc = GetDC(hparent);
  if (hdc==NULL) return E_FAIL;
  RECT rc; GetClientRect(hparent,&rc);
  Render(pData, hdc, &rc);
  ReleaseDC(hparent, hdc);
  return S_OK;
}


// MediaInfo: called whenever the song changes
STDMETHODIMP TStickyViz::MediaInfo(LONG lChannelCount, LONG lSampleRate, BSTR bstrTitle)
{ lChannelCount; lSampleRate; // unused
  char c[1000]; c[0]=0; WideCharToMultiByte(CP_ACP,0,bstrTitle,-1,c,1000,NULL,NULL);
  mtitle=string(c); martist=""; mfilename=""; misnew=true;
  // The issue is that these things might get extra information subsequently
  // through a call to NotifyNewMedia (if the user has WMP9). So we shouldn't
  // do anything yet, with our limited current information. Instead, we
  // just set the misnew flag, so that the next call to Render can do the changes.
  //  
  GotSoundYet=false; prevtime=0; calctime=0;
  return S_OK;
}

void TStickyViz::NewMedia(const string artist, const string title, const string filename)
{ if (recents.size()==0) load_recents(&recents);
  curregexp="";
  if (artist!="" || title!="" || filename!="")
  { load_rules(&rules,false);
    curregexp = match_rule(&rules,artist,title,filename);
    if (curregexp=="") add_recent(&recents,artist,title,filename,"");
  }
  
  if (preset==0) {EnsureDirectoryUpToDate(); SelectBody();} // random
  if (hfull!=0 && preset!=0 && mtitle!="")
  { banner=mtitle;
    bantime = GetTickCount()+3000;
  }
}

// GetCapabilities: see the EFFECT_ flags defined above
STDMETHODIMP TStickyViz::GetCapabilities(DWORD *pdwCapabilities)
{ if (pdwCapabilities==NULL) return E_POINTER;
  *pdwCapabilities = EFFECT_CANGOFULLSCREEN|EFFECT_HASPROPERTYPAGE;
  return S_OK;
}

// GetTitle: the title of the viz
STDMETHODIMP TStickyViz::GetTitle(BSTR* bstrTitle)
{ if (bstrTitle==NULL) return E_POINTER;
  *bstrTitle=SysAllocString(L"Sticky");
  return S_OK;
}






  

BOOL CALLBACK OtherDlgProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ switch (msg)
  { case WM_INITDIALOG:
    { 
      #pragma warning( push )
      #pragma warning( disable: 4244 )
      SetWindowLongPtr(hwnd,DWLP_USER,(LONG_PTR)lParam);
      #pragma warning( pop )
      TStickyViz *viz = (TStickyViz*)lParam;
      CheckDlgButton(hwnd,105,viz->MyRight?BST_CHECKED:BST_UNCHECKED);
      CheckDlgButton(hwnd,106,viz->MyRight?BST_UNCHECKED:BST_CHECKED);
    } return TRUE;
    case WM_COMMAND:
    { int id=LOWORD(wParam);
      if (id==IDCANCEL) EndDialog(hwnd,IDCANCEL);
      if (id!=IDOK) return TRUE;
      TStickyViz *viz = (TStickyViz*)(LONG_PTR)GetWindowLongPtr(hwnd,DWLP_USER);
      bool newright = (IsDlgButtonChecked(hwnd,105)==BST_CHECKED);
      if (newright!=viz->MyRight) {viz->MyRight=newright; viz->RegSave();}
      EndDialog(hwnd,IDOK);
    } return TRUE;
  }
  return FALSE;
}


void TStickyViz::OnRightClick(HWND hwnd,int mx,int my)
{ EnsureDirectoryUpToDate();
  HMENU hmenu=CreatePopupMenu();
  // we'll check if the WMP-owned hwnd is fullscreen or not
  // That's because our "viz-menu" dialog worked badly in fullscreen. I don't know why.
  RECT rc; GetWindowRect(hwnd,&rc);
  bool isfull = (rc.right==GetSystemMetrics(SM_CXSCREEN) && rc.bottom*2>GetSystemMetrics(SM_CYSCREEN) && rc.left==0 && rc.top==0);
  if (!isfull)
  { MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi);
    mi.fMask=MIIM_TYPE|MIIM_ID|MIIM_STATE; mi.fState=MFS_ENABLED;
    mi.fType=MFT_STRING;
    mi.wID=104; mi.dwTypeData="Other Viz's..."; mi.cch=14; InsertMenuItem(hmenu,0,TRUE,&mi);
    mi.fType=MFT_SEPARATOR; mi.wID=127; InsertMenuItem(hmenu,0,TRUE,&mi);
    mi.fType=MFT_STRING; 
    if (GetAsyncKeyState(VK_CONTROL)<0)
    { mi.wID=103; mi.dwTypeData="Diagnostics"; mi.cch=11; mi.fState=MFS_ENABLED|(showAmp?MFS_CHECKED:0);
      InsertMenuItem(hmenu,0,TRUE,&mi); mi.fState=MFS_ENABLED;
    }
    mi.wID=102; mi.dwTypeData="Download..."; mi.cch=11; InsertMenuItem(hmenu,0,TRUE,&mi);
    mi.wID=101; mi.dwTypeData="Edit..."; mi.cch=7; InsertMenuItem(hmenu,0,TRUE,&mi);
    mi.fType=MFT_SEPARATOR; mi.wID=127; InsertMenuItem(hmenu,0,TRUE,&mi);
  }
  //
  for (int i=(int)pre.size()-1; i>=0; i--)
  { string s = pre[i].desc;
    PopulateMenu(hmenu,s.c_str(),1001+i,(preset==i+1?1:0));
  }
  PopulateMenu(hmenu,"Randomize",1000,(preset==0));
  //
  POINT pt; pt.x=mx; pt.y=my; ClientToScreen(hwnd,&pt);
  HCURSOR holdc = SetCursor(LoadCursor(NULL,IDC_ARROW));
  int cmd=TrackPopupMenu(hmenu,TPM_LEFTALIGN|TPM_TOPALIGN|TPM_RETURNCMD,pt.x,pt.y,0,hwnd,NULL);
  SetCursor(holdc);
  DestroyMenu(hmenu);
  //
  if (cmd>=1000)
  { preset=cmd-1000; SelectBody(); RegSave();
    GotSoundYet=false; needsredraw=true; 
    InvalidateRect(hwnd,NULL,TRUE); return;
  }
  else if (cmd==101)
  { save_recents(&recents);
    ShellExecute(NULL,"open",GetStickDir().c_str(),NULL,NULL,SW_SHOWNORMAL);
  }
  else if (cmd==102) ShellExecute(NULL,"open","http://www.wischik.com/lu/senses/sticky/extrasticks.html",NULL,NULL,SW_SHOWNORMAL);
  else if (cmd==103) { showAmp=(1-showAmp); RegSave();}
  else if (cmd==104) DialogBoxParam(hInstance,"VIZDLG",hwnd,OtherDlgProc,(LPARAM)this);
}


// Presets: each viz has a number of presets. They are displayed
// in a submenu in the Windows Media Player. The Windows Media Player
// will itself save in the registry the number of the currently
// selected preset.
STDMETHODIMP TStickyViz::GetPresetTitle(LONG nPreset, BSTR *bstrPresetTitle)
{ if (bstrPresetTitle==NULL) return E_POINTER;
  EnsureDirectoryUpToDate();
  if (nPreset==0) {*bstrPresetTitle = SysAllocString(L"Randomization"); return S_OK;}
  if (nPreset<=0 || nPreset>(LONG)pre.size()) return E_INVALIDARG;
  string s = pre[nPreset-1].desc;
  wchar_t w[MAX_PATH]; MultiByteToWideChar(CP_ACP,0,s.c_str(),-1,w,MAX_PATH);
  *bstrPresetTitle = SysAllocString(w);
  return S_OK;
}
STDMETHODIMP TStickyViz::GetPresetCount(LONG *pnPresetCount)
{ if (pnPresetCount==NULL) return E_POINTER;
  EnsureDirectoryUpToDate();
  *pnPresetCount = (LONG)pre.size()+1;
  return (S_OK);
}
STDMETHODIMP TStickyViz::SetCurrentPreset(LONG nPreset)
{ EnsureDirectoryUpToDate();
  if ((nPreset<0) || (nPreset>(LONG)pre.size())) return E_INVALIDARG;
  preset = nPreset;
  if (preset==0) NewMedia(martist,mtitle,mfilename); // random
  else SelectBody();
  return (S_OK);
}
STDMETHODIMP TStickyViz::GetCurrentPreset(LONG *pnPreset)
{ if (pnPreset==NULL) return E_POINTER;
  *pnPreset = preset;
  return S_OK;
}


// SetCore: called when the viz has been loaded for use in vizing (not properties)
STDMETHODIMP TStickyViz::SetCore(IWMPCore *pPlayer)
{ pPlayer; // unused
  return S_OK;
}

typedef struct {TStickyViz *viz; WNDPROC oldproc; bool *myright; int count;} TSubclassData;

LRESULT CALLBACK SubclassParentProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ HGLOBAL hglob = (HGLOBAL)GetProp(hwnd,"sticky_dat");
  if (hglob==NULL) return DefWindowProc(hwnd,msg,wParam,lParam);
  TSubclassData *osd=(TSubclassData*)GlobalLock(hglob); TSubclassData sd = *osd;
  GlobalUnlock(hglob); // I made a copy of the structure just so I could GlobalUnlock it now, to be more local in my code
  if (sd.viz==0) return CallWindowProc(sd.oldproc,hwnd,msg,wParam,lParam);
  switch (msg)
  { case WM_RBUTTONDOWN:
    { bool isshift = (wParam&MK_SHIFT)!=0;
      isshift |= (GetAsyncKeyState(VK_SHIFT)<0);
      isshift |= (GetAsyncKeyState(VK_LSHIFT)<0);
      isshift |= (GetAsyncKeyState(VK_RSHIFT)<0);
      bool stealmenu;
      if (*sd.myright) stealmenu=!isshift;
      else stealmenu=isshift;
      LRESULT res;
      if (stealmenu) {sd.viz->OnRightClick(hwnd,LOWORD(lParam),HIWORD(lParam)); res=0;}
      else res=CallWindowProc(sd.oldproc,hwnd,msg,wParam,lParam);
      return res;
    } break;
  }
  return CallWindowProc(sd.oldproc,hwnd,msg,wParam,lParam);
}


// Create: called when the viz has been loaded in for use in vizing.
// if hwndParent!=NULL, then RenderWindowed() will be called and we'll have
// to draw into the window specified by hwndParent.
// if hwndParent==NULL, then Render() will be called and we'll have to
// draw into the DC passed to Render().
STDMETHODIMP TStickyViz::Create(HWND hwndParent)
{ hparent = hwndParent;

  // subclass the parent
  HGLOBAL hold = (HGLOBAL)GetProp(hparent,"sticky_dat");
  if (hold==NULL)
  { HGLOBAL hglob = GlobalAlloc(GMEM_MOVEABLE,sizeof(TSubclassData));
    TSubclassData *sd = (TSubclassData*)GlobalLock(hglob);
    sd->oldproc = (WNDPROC)(LONG_PTR)GetWindowLongPtr(hparent,GWLP_WNDPROC);
    sd->viz = this; sd->myright=&MyRight; sd->count=1;
    GlobalUnlock(hglob);
    SetProp(hparent,"sticky_dat",hglob);
    SetWindowLongPtr(hparent,GWLP_WNDPROC,(LONG)(LONG_PTR)SubclassParentProc);
  }
  else // if it had been subclassed before, we merely need to tell it the new thing
  { TSubclassData *sd = (TSubclassData*)GlobalLock(hold);
    sd->viz = this; sd->myright=&MyRight; sd->count++;
    GlobalUnlock(hold);
  }

  return S_OK;
}
STDMETHODIMP TStickyViz::Destroy()
{ // remove the subclass.
  // There's a problem here. If an old viz had been selected before, and now
  // our viz is chosen, then WMP sends us Create then Create then Destroy.
  // (and much later, upon closing, a Destroy to match the first Create).
  // We use the field 'count' to protect against that internal create/destroy
  HGLOBAL hold = (HGLOBAL)GetProp(hparent,"sticky_dat");
  if (hold!=NULL) 
  { TSubclassData *sd = (TSubclassData*)GlobalLock(hold);
    sd->count--;
    if (sd->count==0) {sd->viz = 0; sd->myright=0;}
    GlobalUnlock(hold);
  }

  return S_OK;
}

// NotifyNewMedia: called when a new media stream beings playing
STDMETHODIMP TStickyViz::NotifyNewMedia(IWMPMedia *media)
{ if (media==NULL) {martist=""; mtitle=""; mfilename=""; misnew=true; return S_OK;}
  //
  BSTR val; char c[1000]; HRESULT hres;
  BSTR keya=SysAllocString(L"Author"), keyt=SysAllocString(L"Title"), keyf=SysAllocString(L"SourceURL"), keyp=SysAllocString(L"WM/Picture"), keyl=SysAllocString(L"");
  hres = media->getItemInfo(keya,&val); if (hres==S_OK)
  { c[0]=0; WideCharToMultiByte(CP_ACP,0,val,-1,c,1000,NULL,NULL); SysFreeString(val);
    martist=c;
  }
  hres = media->getItemInfo(keyt,&val); if (hres==S_OK)
  { c[0]=0; WideCharToMultiByte(CP_ACP,0,val,-1,c,1000,NULL,NULL); SysFreeString(val);
    mtitle=c;
  }
  hres = media->getItemInfo(keyf,&val); if (hres==S_OK)
  { c[0]=0; WideCharToMultiByte(CP_ACP,0,val,-1,c,1000,NULL,NULL); SysFreeString(val);
    mfilename=c;
  }
  // We'd have liked to get the album art. But unfortunately the WM/Picture
  // property only applies to album art embedded within the file, not to art
  // downloaded over the internet, so there's not much use.
  //IWMPMedia3 *media3;
  //hres = media->QueryInterface(IID_IWMPMedia3,(void**)&media3); if (hres==S_OK)
  //{ VARIANT var;
  //  LONG count;
  //  hres = media3->getAttributeCountByType(keyp,keyl,&count); if (hres==S_OK && count>0)
  //  { hres = media3->getItemInfoByType(keyp,keyl,0,&var); if (hres==S_OK)
  //    { IUnknown *unk = var.punkVal;
  //      IWMPMetadataPicture *ipic;
  //      hres = unk->QueryInterface(IID_IWMPMetadataPicture,(void**)&ipic); if (hres==S_OK)
  //      { BSTR mime=0,type=0,desc=0,url=0;
  //        ipic->get_mimeType(&mime);
  //        ipic->get_pictureType(&type);
  //        ipic->get_description(&desc);
  //        ipic->get_URL(&url);
  //        if (mime!=0) SysFreeString(mime);
  //        if (type!=0) SysFreeString(type);
  //        if (desc!=0) SysFreeString(desc);
  //        if (url!=0) SysFreeString(url);
  //        ipic->Release();
  //      }
  //      unk->Release();
  //    }
  //  }
  //media3->Release();
  //}
  SysFreeString(keya); SysFreeString(keyt); SysFreeString(keyf); SysFreeString(keyp); SysFreeString(keyl);
  misnew=true;
  //
  // lazy updating of media info. (see comments in MediaInfo method)
  // The following code also appears in Render. But by doing it here,
  // we can do it at a more appropriate time. (i.e. before the rendering has started!)
  if (misnew) {NewMedia(martist,mtitle,mfilename); misnew=false;}

  return S_OK;
}

// OnWindowMessage: window message sent to the parent window
STDMETHODIMP TStickyViz::OnWindowMessage(UINT msg,WPARAM wParam,LPARAM lParam,LRESULT *plResult)
{ msg; wParam; lParam; plResult; // unused
  return S_FALSE;
}






typedef struct {vector<TPre> pre; vector<TBody*> b; vector<bool> tried; HANDLE fchange; RECT rc; int pic, off, width, height; HFONT huf; bool MyRight; TStickyViz *viz;} TPropStuff;

// Note: the config dialog uses a DIFFERENT INSTANCE of TStickyViz from
// the currently-rendering instance. Doesn't matter, since they don't communicate.
BOOL APIENTRY PropDlgProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ TPropStuff *ps=0;
  if (msg!=WM_INITDIALOG) ps = (TPropStuff*)(LONG_PTR)GetWindowLongPtr(hwnd,DWLP_USER);
  if (msg!=WM_INITDIALOG && ps==0) return FALSE;
  switch (msg) 
  { case WM_INITDIALOG:
    { PROPSHEETPAGE *psp = (PROPSHEETPAGE*)lParam;
      ps = (TPropStuff*) (psp->lParam);
      SetWindowLongPtr(hwnd,DWLP_USER,(LONG)(LONG_PTR)ps);
      void SetDlgItemUrl(HWND hdlg,int id,const char *url); 
      SetDlgItemUrl(hwnd,104,"http://www.wischik.com/lu/senses/sticky/extrasticks.html");
      SendMessage(hwnd,WM_USER,0,0); // populate 'pre' and reset 'b'
      ps->fchange = FindFirstChangeNotification(GetStickDir().c_str(),TRUE,FILE_NOTIFY_CHANGE_FILE_NAME|FILE_NOTIFY_CHANGE_DIR_NAME|FILE_NOTIFY_CHANGE_SIZE|FILE_NOTIFY_CHANGE_LAST_WRITE);
      GetClientRect(GetDlgItem(hwnd,103),&ps->rc);
      ps->width=ps->rc.right; ps->height=ps->rc.bottom;
      ps->pic=0; ps->off=ps->height;
      //
      CheckDlgButton(hwnd,105,ps->MyRight?BST_CHECKED:BST_UNCHECKED);
      CheckDlgButton(hwnd,106,ps->MyRight?BST_UNCHECKED:BST_CHECKED);
      //
      SendMessage(hwnd,WM_TIMER,0,0);
      SetTimer(hwnd,1,100,NULL);
      return TRUE;
    }
    case WM_USER:
    { ps->pre.clear();
      for (vector<TBody*>::iterator i=ps->b.begin(); i!=ps->b.end(); i++) {if (*i!=0) delete *i; *i=0;}
      ps->b.clear(); ps->tried.clear();
      DirScan(ps->pre,GetStickDir()+"\\");
      //for (vector<TPre>::iterator j=ps->pre.begin(); j!=ps->pre.end(); j++)
      //{ TBody *b=new TBody(); char err[1000];
      //  bool res = OpenFile(&b,j->path,err);
      //  if (!res) {delete b; b=0;}
      //  ps->b.push_back(b);
      //}
      ps->b.resize(ps->pre.size()); ps->tried.resize(ps->pre.size());
      if (ps->pre.size()==0)
      { EnableWindow(GetDlgItem(hwnd,102),FALSE);
        ShowWindow(GetDlgItem(hwnd,101),SW_SHOW);
      }
      else
      { EnableWindow(GetDlgItem(hwnd,102),TRUE);
        ShowWindow(GetDlgItem(hwnd,101),SW_HIDE);
      }
      return TRUE;
    }
    case WM_TIMER:
    { DWORD res = WaitForSingleObject(ps->fchange,0);
      if (res==WAIT_OBJECT_0)
      { SendMessage(hwnd,WM_USER,0,0);
        FindNextChangeNotification(ps->fchange);
      }
      HDC hdc=GetDC(GetDlgItem(hwnd,103));
      int x=-ps->off, tpic=ps->pic;
      double freq[3][6];
      for (int chan=0; chan<2; chan++)
      { for (int band=0; band<6; band++)
        { double d=(double)(rand()%1000)/1000;
          d=d*d;
          freq[chan][band]=d;
        }
      }
      freq[2][0]=freq[0][3]; freq[2][1]=freq[1][4]; // *** karaoke
      //
      while (x<=ps->width)
      { TBody *b = 0;
        if (tpic<(int)(ps->b.size()))
        { if (!ps->tried[tpic])
          { TBody *b=new TBody(); char err[1000];
            bool res = OpenFile(&b,ps->pre[tpic].path,err);
            if (!res) {delete b; b=0;}
            ps->b[tpic]=b;
            ps->tried[tpic]=true;
          }
          b = ps->b[tpic];
        }
        if (b!=0)
        { b->AssignFreq(freq,1.0);
          b->RecalcEffects(); b->Recalc();
          // draw it at (x,0)-(x+height,height)
          RECT brc; brc.left=x; brc.top=0; brc.right=x+ps->height; brc.bottom=ps->height;
          HRGN hrgn = CreateRectRgn(brc.left,brc.top,brc.right,brc.bottom);
          SelectClipRgn(hdc,hrgn);
          SimpleDraw(hdc,brc,b);
          SelectClipRgn(hdc,NULL);
          DeleteObject(hrgn);
        }
        else
        { RECT brc; brc.left=x; brc.top=0; brc.right=x+ps->height; brc.bottom=ps->height;
          FillRect(hdc,&brc,(HBRUSH)GetStockObject(BLACK_BRUSH));
        }
        // and draw a black spacer
        RECT rc; rc.left=x+ps->height, rc.top=0; rc.right=x+ps->height*3/2; rc.bottom=ps->height;
        FillRect(hdc,&rc,(HBRUSH)GetStockObject(BLACK_BRUSH));
        tpic++; if (tpic>=(int)(ps->b.size())) tpic=0;
        x += ps->height*3/2;
      }
      ps->off += (ps->height/25);
      if (ps->off>=ps->height*3/2)
      { ps->off -= ps->height*3/2;
        ps->pic++; if (ps->pic>=(int)(ps->b.size())) ps->pic=0;
      }
      ReleaseDC(GetDlgItem(hwnd,103),hdc);
      return TRUE;
    }
    case WM_COMMAND:
    { WORD id = LOWORD(wParam), code=HIWORD(wParam);
      if (id==102 && code==BN_CLICKED)
      { save_recents(&recents);
        string dir = GetStickDir();
        ShellExecute(hwnd,"open",dir.c_str(),0,0,SW_SHOWNORMAL);
        return TRUE;
      }
      return FALSE;
    }
    case WM_NOTIFY:
    { NMHDR *nmh = (NMHDR*)lParam; int code=nmh->code;
      if (code==PSN_KILLACTIVE) SetWindowLong(hwnd,DWL_MSGRESULT,FALSE);
      else if (code==PSN_APPLY)
      { bool x = (IsDlgButtonChecked(hwnd,105)==BST_CHECKED);
        ps->viz->PropSheetOkay(x);
        SetWindowLong(hwnd,DWL_MSGRESULT,PSNRET_NOERROR);
      }
      return TRUE;
    }
    case WM_DESTROY:
    { if (ps->fchange!=0) FindCloseChangeNotification(ps->fchange); ps->fchange=0;
      KillTimer(hwnd,1);
      for (vector<TBody*>::iterator i=ps->b.begin(); i!=ps->b.end(); i++) {if (*i!=0) delete *i; *i=0;}
      ps->b.clear();
      return TRUE;
    }
  }
  return FALSE;
}




// DisplayPropertyPage. For when the user clicks on Tools|Options|Visuations|MyViz|Properties.
STDMETHODIMP TStickyViz::DisplayPropertyPage(HWND hwndOwner)
{ PROPSHEETPAGE psp[1];
  PROPSHEETHEADER psh;
  TPropStuff ps; ps.viz=this; ps.MyRight=MyRight;
  psp[0].dwSize = sizeof(PROPSHEETPAGE);
  psp[0].dwFlags = PSP_USEICONID | PSP_USETITLE;
  psp[0].hInstance = hInstance;
  psp[0].pszTemplate = "PropDlg";
  psp[0].pszIcon = 0;
  psp[0].pfnDlgProc = PropDlgProc;
  psp[0].pszTitle = "Sticky";
  psp[0].lParam = (LPARAM)&ps;
  psp[0].pfnCallback = NULL;
  psh.dwSize = sizeof(PROPSHEETHEADER);
  psh.dwFlags = PSH_USEICONID | PSH_PROPSHEETPAGE;
  psh.hwndParent = hwndOwner;
  psh.hInstance = hInstance;
  psh.pszIcon = MAKEINTRESOURCE(2);
  psh.pszCaption = (LPSTR) "Sticky";
  psh.nPages = sizeof(psp) / sizeof(PROPSHEETPAGE);
  psh.nStartPage = 0;
  psh.ppsp = (LPCPROPSHEETPAGE) &psp;
  psh.pfnCallback = NULL;
  PropertySheet(&psh);
  return S_OK;
};




// -----------------------------------------------------------------------------
// TStickyVizFactory and Dll* functions. These are all part of COM.
//
class TStickyVizFactory : public IClassFactory
{
protected:
  long ref;
public:
  // IUnknown... (nb. this class is instantiated statically, which is why Release() doesn't delete it.)
  HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void **ppv) {if (riid==IID_IUnknown || riid==IID_IClassFactory) {*ppv=this; AddRef(); return S_OK;} else return E_NOINTERFACE;}
  ULONG STDMETHODCALLTYPE AddRef() {InterlockedIncrement(&gref); return InterlockedIncrement(&ref);}
  ULONG STDMETHODCALLTYPE Release() {int tmp = InterlockedDecrement(&ref); InterlockedDecrement(&gref); return tmp;}
  // IClassFactory...
  HRESULT STDMETHODCALLTYPE LockServer(BOOL b) {if (b) InterlockedIncrement(&gref); else InterlockedDecrement(&gref); return S_OK;}
  HRESULT STDMETHODCALLTYPE CreateInstance(LPUNKNOWN pUnkOuter, REFIID riid, LPVOID *ppvObj)
  { *ppvObj = NULL; if (pUnkOuter) return CLASS_E_NOAGGREGATION;
    TStickyViz *viz=new TStickyViz(); viz->AddRef(); // We do an addref/release
    HRESULT hr=viz->QueryInterface(riid, ppvObj);    // trick so that it gets
    viz->Release(); return hr;                       // deleted if QueryInt failed.
  }
  // TStickyVizFactory...
  TStickyVizFactory() : ref(0) {}
  ~TStickyVizFactory() {}
};



STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID *ppvOut)
{ static TStickyVizFactory fac; *ppvOut = NULL;
  if (rclsid==StickyCLSID) {return fac.QueryInterface(riid,ppvOut);}
  else return CLASS_E_CLASSNOTAVAILABLE;
}

STDAPI DllCanUnloadNow(void) {return (gref>0)?S_FALSE:S_OK;}

STDAPI DllRegisterServer(void)
{ HKEY hkey,hskey; DWORD disp; LONG res; char c[MAX_PATH];
  char clsid[MAX_PATH]; wsprintf(clsid,"{%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X}",StickyCLSID.Data1,StickyCLSID.Data2,StickyCLSID.Data3,StickyCLSID.Data4[0],StickyCLSID.Data4[1],StickyCLSID.Data4[2],StickyCLSID.Data4[3],StickyCLSID.Data4[4],StickyCLSID.Data4[5],StickyCLSID.Data4[6],StickyCLSID.Data4[7],StickyCLSID.Data4[8]);
  char fn[MAX_PATH]; GetModuleFileName(hInstance,fn,MAX_PATH);
  // First, set up reg-key under HKEY_CLASSES_ROOT...
  strcpy(c,"CLSID\\"); strcat(c,clsid);
  res = RegCreateKeyEx(HKEY_CLASSES_ROOT,c,0,NULL,0,KEY_WRITE,NULL,&hkey,&disp);
  if (res!=ERROR_SUCCESS) return E_UNEXPECTED;
  RegSetValueEx(hkey,NULL,0,REG_SZ,(LPBYTE)"Sticky",7);
  res = RegCreateKeyEx(hkey,"InProcServer32",0,NULL,0,KEY_WRITE,NULL,&hskey,&disp);
  if (res!=ERROR_SUCCESS) {RegCloseKey(hkey);return E_UNEXPECTED;}
  RegSetValueEx(hskey,NULL,0,REG_SZ,(LPBYTE)fn,(int)strlen(fn)+1);
  RegSetValueEx(hskey,"ThreadingModel",0,REG_SZ,(LPBYTE)"Apartment",10);
  RegCloseKey(hskey);
  RegCloseKey(hkey);
  // Second, set up reg-key under MediaPlayer\\plugins...
  strcpy(c,"SOFTWARE\\Microsoft\\MediaPlayer\\Objects\\Effects\\Sticky");
  res = RegCreateKeyEx(HKEY_LOCAL_MACHINE,c,0,NULL,0,KEY_WRITE,NULL,&hkey,&disp);
  if (res!=ERROR_SUCCESS) return E_UNEXPECTED;
  res = RegCreateKeyEx(hkey,"Properties",0,NULL,0,KEY_WRITE,NULL,&hskey,&disp);
  if (res!=ERROR_SUCCESS) {RegCloseKey(hkey);return E_UNEXPECTED;}
  RegSetValueEx(hskey,"classid",0,REG_SZ,(LPBYTE)clsid,(int)strlen(clsid)+1);
  RegSetValueEx(hskey,"name",0,REG_SZ,(LPBYTE)"Sticky",7);
  RegSetValueEx(hskey,"description",0,REG_SZ,(LPBYTE)"Dancing Stick Figures",22);
  RegCloseKey(hskey);
  RegCloseKey(hkey);
  return S_OK;
}

STDAPI DllUnregisterServer(void)
{ // We run-time link to shlwapi wherein is found the recursive SHDeleteKey function. (Requires Win98/ME/2k/XP)
  typedef DWORD (STDAPICALLTYPE SHDELETEKEY)(HKEY,LPCTSTR); SHDELETEKEY *pSHDeleteKey;
  HINSTANCE hlib = LoadLibrary("shlwapi.dll");
  if (hlib==0) return E_UNEXPECTED;
  pSHDeleteKey=(SHDELETEKEY*)GetProcAddress(hlib,"SHDeleteKeyA");
  if (pSHDeleteKey==0) {FreeLibrary(hlib);return E_UNEXPECTED;}
  char c[MAX_PATH], clsid[MAX_PATH]; wsprintf(clsid,"{%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X}",StickyCLSID.Data1,StickyCLSID.Data2,StickyCLSID.Data3,StickyCLSID.Data4[0],StickyCLSID.Data4[1],StickyCLSID.Data4[2],StickyCLSID.Data4[3],StickyCLSID.Data4[4],StickyCLSID.Data4[5],StickyCLSID.Data4[6],StickyCLSID.Data4[7],StickyCLSID.Data4[8]);
  strcpy(c,"CLSID\\"); strcat(c,clsid);
  pSHDeleteKey(HKEY_CLASSES_ROOT,c);
  strcpy(c,"SOFTWARE\\Microsoft\\MediaPlayer\\Objects\\Effects\\Sticky");
  pSHDeleteKey(HKEY_LOCAL_MACHINE,c);
  FreeLibrary(hlib);
  return S_OK;
}

BOOL WINAPI DllMain(HINSTANCE h, DWORD reason, LPVOID)
{ if (reason==DLL_PROCESS_ATTACH) hInstance=h;
  return TRUE;
}








// --------------------------------------------------------------------------------
// SetDlgItemUrl(hwnd,IDC_MYSTATIC,"http://www.wischik.com/lu");
//   This routine turns a dialog's static text control into an underlined hyperlink.
//   You can call it in your WM_INITDIALOG, or anywhere.
//   It will also set the text of the control... if you want to change the text
//   back, you can just call SetDlgItemText() afterwards.
// --------------------------------------------------------------------------------
void SetDlgItemUrl(HWND hdlg,int id,const char *url); 

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
typedef struct {char *url; WNDPROC oldproc; HFONT hf; HBRUSH hb;} TUrlData;
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
void SetDlgItemUrl(HWND hdlg,int id,const char *url) 
{ // nb. vc7 has crummy warnings about 32/64bit. My code's perfect! That's why I hide the warnings.
  #pragma warning( push )
  #pragma warning( disable: 4312 4244 )
  // First we'll subclass the edit control
  HWND hctl = GetDlgItem(hdlg,id);
  SetWindowText(hctl,url);
  HGLOBAL hold = (HGLOBAL)GetProp(hctl,"href_dat");
  if (hold!=NULL) // if it had been subclassed before, we merely need to tell it the new url
  { TUrlData *ud = (TUrlData*)GlobalLock(hold);
    delete[] ud->url;
    ud->url=new char[strlen(url)+1]; strcpy(ud->url,url);
  }
  else
  { HGLOBAL hglob = GlobalAlloc(GMEM_MOVEABLE,sizeof(TUrlData));
    TUrlData *ud = (TUrlData*)GlobalLock(hglob);
    ud->oldproc = (WNDPROC)GetWindowLongPtr(hctl,GWLP_WNDPROC);
    ud->url=new char[strlen(url)+1]; strcpy(ud->url,url);
    ud->hf=0; ud->hb=0;
    GlobalUnlock(hglob);
    SetProp(hctl,"href_dat",hglob);
    SetWindowLongPtr(hctl,GWLP_WNDPROC,(LONG_PTR)UrlCtlProc);
  }
  //
  // Second we subclass the dialog
  hold = (HGLOBAL)GetProp(hdlg,"href_dlg");
  if (hold==NULL)
  { HGLOBAL hglob = GlobalAlloc(GMEM_MOVEABLE,sizeof(TUrlData));
    TUrlData *ud = (TUrlData*)GlobalLock(hglob);
    ud->url=0;
    ud->oldproc = (WNDPROC)GetWindowLongPtr(hdlg,GWLP_WNDPROC);
    ud->hb=CreateSolidBrush(GetSysColor(COLOR_BTNFACE));
    ud->hf=0; // the font will be created lazilly, the first time WM_CTLCOLORSTATIC gets called
    GlobalUnlock(hglob);
    SetProp(hdlg,"href_dlg",hglob);
    SetWindowLongPtr(hdlg,GWLP_WNDPROC,(LONG_PTR)UrlDlgProc);
  }
  #pragma warning( pop )
}

// UrlCtlProc: this is the subclass procedure for the static control
LRESULT CALLBACK UrlCtlProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ HGLOBAL hglob = (HGLOBAL)GetProp(hwnd,"href_dat");
  if (hglob==NULL) return DefWindowProc(hwnd,msg,wParam,lParam);
  TUrlData *oud=(TUrlData*)GlobalLock(hglob); TUrlData ud=*oud;
  GlobalUnlock(hglob); // I made a copy of the structure just so I could GlobalUnlock it now, to be more local in my code
  switch (msg)
  { case WM_DESTROY:
    { RemoveProp(hwnd,"href_dat"); GlobalFree(hglob);
      if (ud.url!=0) delete[] ud.url;
      // nb. remember that ud.url is just a pointer to a memory block. It might look weird
      // for us to delete ud.url instead of oud->url, but they're both equivalent.
    } break;
    case WM_LBUTTONDOWN:
    { HWND hdlg=GetParent(hwnd); if (hdlg==0) hdlg=hwnd;
      ShellExecute(hdlg,"open",ud.url,NULL,NULL,SW_SHOWNORMAL);
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
{ HGLOBAL hglob = (HGLOBAL)GetProp(hwnd,"href_dlg");
  if (hglob==NULL) return DefWindowProc(hwnd,msg,wParam,lParam);
  TUrlData *oud=(TUrlData*)GlobalLock(hglob); TUrlData ud=*oud;
  GlobalUnlock(hglob);
  switch (msg)
  { case WM_DESTROY:
    { RemoveProp(hwnd,"href_dlg"); GlobalFree(hglob);
      if (ud.hb!=0) DeleteObject(ud.hb);
      if (ud.hf!=0) DeleteObject(ud.hf);
    } break;
    case WM_CTLCOLORSTATIC:
    { HDC hdc=(HDC)wParam; HWND hctl=(HWND)lParam;
      // To check whether to handle this control, we look for its subclassed property!
      HANDLE hprop=GetProp(hctl,"href_dat"); if (hprop==NULL) return CallWindowProc(ud.oldproc,hwnd,msg,wParam,lParam);
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



