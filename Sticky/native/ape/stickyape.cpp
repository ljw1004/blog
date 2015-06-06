#include <windows.h>
//#include <commdlg.h>
//#include <commctrl.h>
//#include <shellapi.h>
//#pragma warning( push )
//#pragma warning( disable: 4201 )
//#include <mmsystem.h>
//#pragma warning( pop )
#include <string>
#include <list>
#include <vector>
using namespace std;
#include <math.h>
//#include <stdlib.h>
//#include <stdio.h>
#include "../body.h"
#include "../utils.h"


HINSTANCE hInstance;


// base class to derive from
class C_RBASE
{ public:
	C_RBASE() {}
	virtual ~C_RBASE() {};
	virtual int render(char visdata[2][2][576], int isBeat, int *framebuffer, int *fbout, int w, int h)=0;
	virtual HWND conf(HINSTANCE, HWND) {return 0;};
	virtual char *get_desc()=0;
	virtual void load_config(unsigned char *, int) {}
	virtual int  save_config(unsigned char *) {return 0;}
};


enum OutMode {omReplace=0, omAdd=1, omBlend=2, omOnTop=3};
const char *OutModes[] = {"Replace","Additive","50/50"};

struct ApeDat
{ ApeDat() : enabled(true), om(omReplace), showAmp(false) {*fn=0;}
  //
  bool enabled;
  char fn[MAX_PATH]; // relative filename, eg. "a.stk" or "subdir\b.stk"
  OutMode om;
  bool showAmp;
};


class StickyApe : public C_RBASE 
{ public:
  ApeDat dat;
  stk::TBody *b; bool btried; HBITMAP hbm; int bw,bh;
  CRITICAL_SECTION critsec; unsigned int prevtime,prevfade,fpsp;
  double max[6]; // record the maximum intensity we've received (by frequency)
  double min[6]; // and the maximum
  double kmin[6],kmax[6];
  int period[10],periodi; // for fps counter
  bool GotSoundYet;
  //
	StickyApe() : b(0), btried(false), hbm(0)
  { InitializeCriticalSection(&critsec);
    fpsp=0; periodi=0; prevtime=0; prevfade=0; GotSoundYet=false;
    max[0]=2600; min[0]=150; 
    max[1]=2300; min[1]=140; 
    max[2]=2100; min[2]=130; 
    max[3]=1900; min[3]=120;    
    max[4]=1700; min[4]=110;    
    max[5]=1500; min[5]=100;    
    kmax[0]=400; kmin[0]=20;
    kmax[1]=400; kmin[1]=20;
  }
	virtual ~StickyApe() {if (b!=0) delete b; b=0; if (hbm!=0) DeleteObject(hbm); hbm=0; DeleteCriticalSection(&critsec);}
	virtual int render(char visdata[2][2][576], int isBeat, int *framebuffer, int *fbout, int w, int h);
  virtual HWND conf(HINSTANCE hInstance, HWND hwndParent);
	virtual char *get_desc() {return "Render / Sticky";}
	virtual void load_config(unsigned char *data, int len);
	virtual int  save_config(unsigned char *data);
};

string banner; unsigned int bantime=0;
void stk::luassertmsg(const char *s,const char *msg,const char *f,unsigned int l)
{ char c[2000]; wsprintf(c,"%s '%s' - %s:%u",msg,s,f,l);
  banner=c; bantime=GetTickCount()+3000;
}



void StickyApe::load_config(unsigned char *buf, int len) 
{ if (len<sizeof(dat)) return;
  memcpy(&dat,buf,sizeof(dat));
  if (b!=0) delete b; b=0; btried=false; fpsp=0;
}

int StickyApe::save_config(unsigned char *buf) 
{ memcpy(buf,&dat,sizeof(dat));
  return sizeof(dat);
}


void ApeDirScan(const string root, const string prefix, list<string> *fns)
{ // root = "c:\winamp\avs"
  // prefix = "subdir\" or ""
  WIN32_FIND_DATA fdat; string match = root+"\\"+prefix+"*.*";
  HANDLE hfind=FindFirstFile(match.c_str(),&fdat);
  for (BOOL res=(hfind!=INVALID_HANDLE_VALUE); res; res=FindNextFile(hfind,&fdat))
  { bool isdir = (fdat.dwFileAttributes&FILE_ATTRIBUTE_DIRECTORY)!=0;
    bool isstk = (stk::StringLower(stk::ExtractFileExt(fdat.cFileName))==".stk");
    if (strcmp(fdat.cFileName,".")==0) isdir=false;
    if (strcmp(fdat.cFileName,"..")==0) isdir=false;
    if (isdir) ApeDirScan(root,prefix+fdat.cFileName+"\\",fns);
    else if (isstk) fns->push_back(prefix+fdat.cFileName);
  }
  if (hfind!=INVALID_HANDLE_VALUE) FindClose(hfind);
}

BOOL CALLBACK DlgProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ if (msg==WM_INITDIALOG) SetWindowLong(hwnd,DWL_USER,lParam);
  StickyApe *ape = (StickyApe*)GetWindowLong(hwnd,DWL_USER);
  //
  switch (msg)
	{ case WM_INITDIALOG:
    { void SetDlgItemUrl(HWND hdlg,int id,const char *url); 
      SetDlgItemUrl(hwnd,108,"http://www.wischik.com/lu/senses/sticky");
      SetDlgItemUrl(hwnd,109,"http://www.wischik.com/lu/senses/sticky/extrasticks.html");
      EnableWindow(GetDlgItem(hwnd,106),ape->dat.fn[0]!=0);
      CheckDlgButton(hwnd,101,ape->dat.enabled?BST_CHECKED:BST_UNCHECKED);
      CheckDlgButton(hwnd,107,ape->dat.showAmp?BST_CHECKED:BST_UNCHECKED);
      CheckDlgButton(hwnd,103, (ape->dat.om==omReplace)?BST_CHECKED:BST_UNCHECKED);
      CheckDlgButton(hwnd,104, (ape->dat.om==omAdd)?BST_CHECKED:BST_UNCHECKED);
      CheckDlgButton(hwnd,105, (ape->dat.om==omBlend)?BST_CHECKED:BST_UNCHECKED);
      CheckDlgButton(hwnd,110, (ape->dat.om==omOnTop)?BST_CHECKED:BST_UNCHECKED);
      char fn[MAX_PATH]; GetModuleFileName(hInstance,fn,MAX_PATH);
      list<string> sticks; ApeDirScan(stk::ExtractFilePath(fn),"",&sticks);
      sticks.sort();
      for (list<string>::const_iterator i=sticks.begin(); i!=sticks.end(); i++)
      { SendDlgItemMessage(hwnd,102,CB_ADDSTRING,0,(LPARAM)(*i).c_str());
      }
      int pos = SendDlgItemMessage(hwnd,102,CB_FINDSTRING,(WPARAM)-1,(LPARAM)ape->dat.fn);
      SendDlgItemMessage(hwnd,102,CB_SETCURSEL,pos,0);
			return TRUE;
    }
		case WM_COMMAND:
    { int id=LOWORD(wParam), code=HIWORD(wParam);
      EnterCriticalSection(&ape->critsec);
      if (id==101 && code==BN_CLICKED) ape->dat.enabled = (IsDlgButtonChecked(hwnd,101)==BST_CHECKED);
      else if (id==107 && code==BN_CLICKED) ape->dat.showAmp = (IsDlgButtonChecked(hwnd,107)==BST_CHECKED);
      else if (id==102 && code==CBN_SELCHANGE)
      { int pos=SendDlgItemMessage(hwnd,102,CB_GETCURSEL,0,0);
        if (pos==CB_ERR) strcpy(ape->dat.fn,"");
        else SendDlgItemMessage(hwnd,102,CB_GETLBTEXT,pos,(LPARAM)ape->dat.fn);
        if (ape->b!=0) delete ape->b; ape->b=0; ape->btried=false; ape->fpsp=0;
        EnableWindow(GetDlgItem(hwnd,106),ape->dat.fn[0]!=0);
      }
      else if (id==103 && code==BN_CLICKED) ape->dat.om = (IsDlgButtonChecked(hwnd,103)==BST_CHECKED)?omReplace:ape->dat.om;
      else if (id==104 && code==BN_CLICKED) ape->dat.om = (IsDlgButtonChecked(hwnd,104)==BST_CHECKED)?omAdd:ape->dat.om;
      else if (id==105 && code==BN_CLICKED) ape->dat.om = (IsDlgButtonChecked(hwnd,105)==BST_CHECKED)?omBlend:ape->dat.om;
      else if (id==110 && code==BN_CLICKED) ape->dat.om = (IsDlgButtonChecked(hwnd,110)==BST_CHECKED)?omOnTop:ape->dat.om;
      else if (id==106 && code==BN_CLICKED)
      { char buf[MAX_PATH]; GetModuleFileName(hInstance,buf,MAX_PATH);
        string fn = stk::ExtractFilePath(buf)+"\\"+ape->dat.fn;
        ShellExecute(hwnd,"edit",fn.c_str(),0,0,SW_SHOWNORMAL);
      }
      LeaveCriticalSection(&ape->critsec);
    }
  }
  return 0;
}



int StickyApe::render(char visdata[2][2][576],int,int *framebuffer, int *, int w, int h)
{ // return 0 if output in framebuffer, or 1 if output in fbout
  // visdata[spectrum=0,wave=1][channel][band]
  if (!dat.enabled) return 0;
  if (hbm!=0 && (bw!=w || bh!=h)) {DeleteObject(hbm); hbm=0;}
  if (hbm==0)
  { BITMAPINFOHEADER bih; ZeroMemory(&bih,sizeof(bih));
    bih.biSize=sizeof(bih);
    bih.biWidth=w;
    bih.biHeight=-h;
    bih.biPlanes=1;
    bih.biBitCount=32;
    bih.biCompression=BI_RGB;
    bih.biSizeImage = ((bih.biWidth*bih.biBitCount/8+3)&0xFFFFFFFC)*bih.biHeight;
    bih.biXPelsPerMeter=10000;
    bih.biYPelsPerMeter=10000;
    bih.biClrUsed=0;
    bih.biClrImportant=0;
    void *bits; hbm=CreateDIBSection(0,(BITMAPINFO*)&bih,DIB_RGB_COLORS,&bits,NULL,NULL);
    bw=w; bh=h;
  }
  HDC sdc=GetDC(0), hdc=CreateCompatibleDC(sdc); ReleaseDC(0,sdc);
  SelectObject(hdc,hbm);
  //
  //
  DWORD nowtime=GetTickCount();
  if (nowtime>=bantime) banner="";
  if (nowtime>=prevtime+fpsp)
  { double cmul = ((double)(nowtime-prevtime))/10.0; // diff as a multiple of 10ms, the standard interval
    prevtime=nowtime;
    //
    EnterCriticalSection(&critsec);
    if (b==0 && !btried)
    { btried=true;
      char buf[MAX_PATH]; GetModuleFileName(hInstance,buf,MAX_PATH);
      string fn = stk::ExtractFilePath(buf)+"\\"+dat.fn;
      b=new stk::TBody();
      char err[1024];
      bool res = stk::LoadBody(&b, fn.c_str(),err,stk::lbForUse);
      fpsp = 1000/b->fps;
      if (!res) {banner=err; delete b; b=0; bantime=nowtime+5000;}
    }

    double l[6], r[6], k[6];
    if (!GotSoundYet)
    { for (int p=0; p<6; p++) {l[p]=0.2; r[p]=0.2;}
    } 
    bool adjust=false;
    if (nowtime>prevfade+100)
    { for (int p=0; p<6; p++)
      { max[p]*=0.999; kmax[p]*=0.997;
        if (min[p]<22) min[p]=22; min[p]=min[p]+1*1.002;
        if (kmin[p]<12) kmin[p]=12; kmin[p]*=1.002;
      }
      adjust=true;
      prevfade=nowtime;
    }
    int boff=3, bwidth=4;
    for (int p=0; p<6; p++) 
    { l[p]=0; r[p]=0; int i;
      for (i=0; i<bwidth; i++)
      { int al = *(unsigned char*)&visdata[0][0][boff+p*bwidth+i];
        int ar = *(unsigned char*)&visdata[0][1][boff+p*bwidth+i];
        l[p] += al*3;
        r[p] += ar*3;
      }
      //l[p] = log(l[p])*300;
      //r[p] = log(r[p])*300;
      if (l[p]>max[p]) max[p]=l[p];
      if (r[p]>max[p]) max[p]=r[p];
      if (adjust)
      { if (l[p]<min[p] || r[p]<min[p]) min[p]*=0.9;
      }
    }
    //
    // the waveform is 576 elements big, stored as unsigned chars.
    // 0.vocals=diff, 1.music=average
    int kvoc=0, kmus=0;
    for (int i=0; i<576; i++)
    { int voc = (visdata[1][0][i]+visdata[1][1][i])/2;
      int mus = (int)(visdata[1][0][i])-(int)(visdata[1][1][i]);
      kvoc += voc*voc; kmus += mus*mus;
    }
    kvoc=(int)sqrt(kvoc); kmus=2*(int)sqrt(kmus);
    k[0]=kvoc; k[1]=kmus;
    if (k[0]<kmin[0]) kmin[0]=k[0];  if (k[0]>kmax[0]) kmax[0]=k[0];
    if (k[1]<kmin[1]) kmin[1]=k[1];  if (k[1]>kmax[1]) kmax[1]=k[1];
    //
    for (int p=0; p<6; p++)
    { l[p] = (l[p]-min[p]) / (max[p]-min[p]+1); if (l[p]<0) l[p]=0; l[p]*=l[p];
      r[p] = (r[p]-min[p]) / (max[p]-min[p]+1); if (r[p]<0) r[p]=0; r[p]*=r[p];
    }
    bool okay=true;
    for (int p=0; p<6; p++)
    { if (l[p]<0 || l[p]>1) okay=false;
      if (r[p]<0 || r[p]>1) okay=false;
    }
    for (int p=0; p<2; p++)
    { k[p] = (k[p]-kmin[p]) / (kmax[p]-kmin[p]+1); if (k[p]<0) k[p]=0; k[p]*=k[p];
    }
    
    for (int p=0; p<6; p++)
    { if (l[p]>0.01 || r[p]>0.01) GotSoundYet=true;
    }
  
    //
    double freq[3][6];
    for (int p=0; p<6; p++) {freq[0][p]=l[p]; freq[1][p]=r[p]; freq[2][p]=k[p];}
    if (b!=0)
    { b->AssignFreq(freq,cmul);
      b->RecalcEffects();
      b->Recalc();
    }
    RECT rc; rc.left=0; rc.top=0; rc.right=bw; rc.bottom=bh;
    stk::TAmpData ad; ad.l=l; ad.r=r; ad.min=min; ad.max=max; ad.k=k; ad.kmin=kmin; ad.kmax=kmax;
    string err; bool ok=stk::SimpleDraw(hdc,rc,b,banner.c_str(),dat.showAmp?&ad:0);
    if (!ok) banner=err;
    LeaveCriticalSection(&critsec);
  }


  DeleteDC(hdc);
  DIBSECTION dibs; GetObject(hbm,sizeof(dibs),&dibs);
  int *wit = (int*)dibs.dsBm.bmBits;
  int wh=w*h;
  //
  switch (dat.om)
  { case omReplace: memcpy(framebuffer,dibs.dsBm.bmBits,w*h*4); break;
    case omOnTop:
    { for (int *src=wit, *end=wit+w*h, *dst=framebuffer; src<end; src++,dst++)
      { if (*src!=0) *dst=*src;
      }
    } break;
		case omAdd: __asm
    { mov ebx, framebuffer;
			mov edx, wit;
			mov ecx, wh;
			shr ecx, 1;
			loop2:
			movq mm0, [edx];
			movq mm1, [ebx];
			paddusb mm0, mm1;
			movq [ebx], mm0;
      add ebx, 8;
			add edx, 8;
      dec ecx;
			jnz loop2;
		} break;
		case omBlend: _asm
		{ mov ebx, framebuffer;
			mov edx, wit;
			mov ecx, wh;
			pxor mm7, mm7;
			loop4:
			movd mm0, [edx];
			movd mm1, [ebx];
			punpcklbw mm0, mm7;
			punpcklbw mm1, mm7;
			paddusw mm0, mm1;
			psrlw mm0, 1;
			packuswb mm0, mm0;
			movd [ebx], mm0;
      add ebx, 4;
			add edx, 4;
      dec ecx;
			jnz loop4;
		}	break;
	}
	__asm
  { emms;
  }
	return 0;
}









//-------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------


HWND StickyApe::conf(HINSTANCE hInstance, HWND hwnd) 
{ return CreateDialogParam(hInstance,"DLG_CONFIG",hwnd,DlgProc,(LPARAM)this);
}

C_RBASE *CallbackFunc(char *desc) 
{ if (desc==0) return new StickyApe();
  strcpy(desc,"Render / Sticky");
  return 0;
}

extern "C" __declspec(dllexport) int _AVS_APE_RetrFunc(HINSTANCE hDllInstance,char **info,int *create)
{ hInstance=hDllInstance;
	*info="WISCHIK.STICKY.APE.STICKY";
	*create=(int)(void*)CallbackFunc;
	return 1;
};



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

