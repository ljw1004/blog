#include <windows.h>
#include <commdlg.h>
#include <commctrl.h>
#include <shellapi.h>
#pragma warning( push )
#pragma warning( disable: 4201 )
#include <mmsystem.h>
#pragma warning( pop )
#include <string>
#include <list>
#include <vector>
using namespace std;
#include <math.h>
#include <stdlib.h>
#include <stdio.h>
#include "../body.h"
#include "../utils.h"
using namespace stk;

LRESULT CALLBACK FullWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam); // for the full-screen window
class TViewer; typedef struct {TViewer *swnd; int nb; int w,h;} TFullWndDat;
typedef struct {int x,y,w,h;} TMonitorInfo; vector<TMonitorInfo> monitors;

HINSTANCE hInstance;

list<string> recents;  // the recent music we have heard




typedef struct tagWinampVisModule {
  char *description; // description of module
  HWND hwndParent;   // parent window (filled in by calling app)
  HINSTANCE hDllInstance; // instance handle to this DLL (filled in by calling app)
  int sRate;		 // sample rate (filled in by calling app)
  int nCh;			 // number of channels (filled in...)
  int latencyMs;     // latency from call of RenderFrame to actual drawing
                     // (calling app looks at this value when getting data)
  int delayMs;       // delay between calls in ms

  // the data is filled in according to the respective Nch entry
  int spectrumNch;
  int waveformNch;
  unsigned char spectrumData[2][576];
  signed char waveformData[2][576];

  void *configfunc;  // configuration dialog
  void *initfunc;     // 0 on success, creates window, etc
  void *renderfunc;  // returns 0 if successful, 1 if vis should end
  void *quitfunc;    // call when done

  void *userData; // user data, optional
} TWinampVisModule;

typedef struct {
  int version;
  char *description;
  TWinampVisModule* (*getModule)(int);
} TWinampVisHeader;

typedef TWinampVisHeader* (*winampVisGetHeaderType)();


// If building without these latest headers, we have to define stuff ourself
#ifndef SM_CMONITORS
DECLARE_HANDLE(HMONITOR);
#define SM_CMONITORS            80
#endif
typedef BOOL (CALLBACK *LUMONITORENUMPROC)(HMONITOR,HDC,LPRECT,LPARAM);
typedef BOOL (WINAPI *LUENUMDISPLAYMONITORS)(HDC,LPCRECT,LUMONITORENUMPROC,LPARAM);







// ------------------------------------------------------------------------------------------
// ------------------------------------------------------------------------------------------
// ------------------------------------------------------------------------------------------
  



class TViewer
{ public:
  TViewer(HWND h); HWND hwnd; 
  ~TViewer();
  void OnCreate(); 
  void OnRightClick(int x,int y);
  void OnSize(int w,int h, int m); bool topmost;
  void OnPaint(PAINTSTRUCT *ps, HDC hdc, int m); vector<HDC> memDCs; vector<HBITMAP> memBMs,oldBMs;
  void OnTick(TWinampVisModule *mod);   // called by winamp every however often
  void Redraw(int m); bool showui,havehiddenui;
  string oldfn; UINT ofnTimer; // part of the "new media event" manager
  void OnNewMedia(const string artist, const string title, const string filename);
  
  // Sticky behaviour
  bool isfull; vector<HWND> fullwnds;
  void StartFull(); void EndFull();
  void StickyInit(); // sets up timings, "body" pointer, &c.
  void StickyEnd();
  void RegLoad();  // sticky stores its colour+timing+other preferences
  void RegSave();  // in the registry
  void SelectBody(bool leave0=false,bool showrand=false); string curboddesc0;     // loads the body based on the "preset" variable
  bool OpenFile(TBody **b,string fn,char *err); // loads the file into the body
  void Calc(TWinampVisModule *mod,DWORD nowtime); // updates l[],r[] arrays
  void UpdateBody();  // given l[],r[] arrays, recalculates limb positions
  void Draw(HDC hdc,RECT &rc,TBody *body,const string banner); double scale; int soffx,soffy;
  void EnsureDirectoryUpToDate();
  void ReadDirectory();  // updates the internal list according to the contents of the stick directory
  HCURSOR hcprev,hcnext,hcfull,hcclose,hchandle;
  // sticky properties
  string martist, mtitle, mfilename, curregexp; // set by OnNewMedia
  list<TRule> rules;      // rules for selecting which stick to show
  int showAmp;            // should we show amplitude sticks?
  int preset;             // index of current stick
  vector<TPre> pre; // all of the presets: their display-names, and full paths. Populated in finalconstruct.
  vector<string> banners; unsigned int bantime; // a text banner, plus when we stop displaying it.
  double l[6],r[6],k[6];  // we divide the spectrum up into 6 bands 
  double cmul;
  bool needsredraw;  // a tick from Winamp merely sets l[6],r[6],needsredraw and calls InvalidateRect.
  double max[6]; // record the maximum intensity we've received (by frequency)
  double min[6]; // and the maximum
  double kmin[6],kmax[6];
  bool GotSoundYet;       // have we yet got any sound?
  DWORD prevtime;         // CPU tick count of the previous calculation
  DWORD calctime;         // CPU tick count for when we should next recalculate limb positions
  int period[10],periodi; // for fps counter
  DWORD periodtime;       // for fps counter, the CPU tick count of the previous frame
  vector<TBody*> bodies; int nbodies;           // the stick figure himself!
  HANDLE hdir;            // notifications when the stick directory changes
};


TViewer *debugv=0;
void stk::luassertmsg(const char *s,const char *msg,const char *f,unsigned int l)
{ if (debugv==0) return; if (debugv->banners.size()==0) return;
  char c[2000]; wsprintf(c,"%s '%s' - %s:%u",msg,s,f,l);
  debugv->banners[0]=c; debugv->bantime=GetTickCount()+3000;
}



TViewer::TViewer(HWND h) : isfull(false), nbodies(0)
{ hwnd=h;
  memDCs.resize(monitors.size()); memBMs.resize(monitors.size()); oldBMs.resize(monitors.size());
  memDCs[0] = CreateCompatibleDC(NULL);
  HDC hdc=GetDC(0); memBMs[0] = CreateCompatibleBitmap(hdc,0,0); ReleaseDC(0,hdc);
  oldBMs[0] = (HBITMAP)SelectObject(memDCs[0],memBMs[0]);
  for (int i=1; i<(int)monitors.size(); i++) {memDCs[i]=0; memBMs[i]=0; oldBMs[i]=0;}
  topmost=false; showui=true; havehiddenui=false;
  hcprev=(HCURSOR)LoadImage(hInstance,"prev",IMAGE_CURSOR,0,0,LR_SHARED);
  hcnext=(HCURSOR)LoadImage(hInstance,"next",IMAGE_CURSOR,0,0,LR_SHARED);
  hcfull=(HCURSOR)LoadImage(hInstance,"full",IMAGE_CURSOR,0,0,LR_SHARED);
  hcclose=(HCURSOR)LoadImage(hInstance,"close",IMAGE_CURSOR,0,0,LR_SHARED);
  hchandle=(HCURSOR)LoadImage(hInstance,"handle",IMAGE_CURSOR,0,0,LR_SHARED);
  //
  StickyInit();
}

TViewer::~TViewer()
{ RECT rc; GetWindowRect(hwnd,&rc);
  HKEY key; DWORD disp; DWORD dat;
  LONG res=RegCreateKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,NULL,0,KEY_WRITE,NULL,&key,&disp);
  if (res==ERROR_SUCCESS)
  { dat=rc.left; RegSetValueEx(key,"left",0,REG_DWORD,(CONST BYTE*)&dat,sizeof(dat));
    dat=rc.right; RegSetValueEx(key,"right",0,REG_DWORD,(CONST BYTE*)&dat,sizeof(dat));
    dat=rc.top; RegSetValueEx(key,"top",0,REG_DWORD,(CONST BYTE*)&dat,sizeof(dat));
    dat=rc.bottom; RegSetValueEx(key,"bottom",0,REG_DWORD,(CONST BYTE*)&dat,sizeof(dat));
    dat=(topmost?1:0); RegSetValueEx(key,"topmost",0,REG_DWORD,(CONST BYTE*)&dat,sizeof(dat));
    dat=(showui?1:0); RegSetValueEx(key,"showui",0,REG_DWORD,(CONST BYTE*)&dat,sizeof(dat));
    dat=(havehiddenui?1:0); RegSetValueEx(key,"havehiddenui",0,REG_DWORD,(CONST BYTE*)&dat,sizeof(dat));
    RegCloseKey(key);
  }
  RegSave();
  //
  if (isfull) EndFull();
  for (int i=0; i<(int)monitors.size(); i++)
  { if (memDCs[i]!=0)
    { SelectObject(memDCs[i],oldBMs[i]);
      DeleteObject(memDCs[i]);
      DeleteObject(memBMs[i]);
    }
    memDCs[i]=0; memBMs[i]=0; oldBMs[i]=0;
  }
  if (hdir!=0) FindCloseChangeNotification(hdir); hdir=0;
  StickyEnd();
}


// ENUM-MONITOR-CALLBACK is part of OnCreate, whose sole task
// is to put the window in a good position

void TViewer::OnCreate()
{ RECT rc; rc.left=100; rc.top=100; rc.bottom=400; rc.right=400;
  HKEY key=NULL; DWORD dat; DWORD size; DWORD type; bool okay=true;
  LONG res=RegOpenKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,KEY_READ,&key);
  if (res!=ERROR_SUCCESS) okay=false;
  else
  { size=sizeof(dat); res=RegQueryValueEx(key,"left",NULL,&type,(LPBYTE)&dat,&size); rc.left=dat; if (res!=ERROR_SUCCESS) okay=false;
    size=sizeof(dat); res=RegQueryValueEx(key,"right",NULL,&type,(LPBYTE)&dat,&size); rc.right=dat; if (res!=ERROR_SUCCESS) okay=false;
    size=sizeof(dat); res=RegQueryValueEx(key,"top",NULL,&type,(LPBYTE)&dat,&size); rc.top=dat; if (res!=ERROR_SUCCESS) okay=false;
    size=sizeof(dat); res=RegQueryValueEx(key,"bottom",NULL,&type,(LPBYTE)&dat,&size); rc.bottom=dat; if (res!=ERROR_SUCCESS) okay=false;
    size=sizeof(dat); res=RegQueryValueEx(key,"topmost",NULL,&type,(LPBYTE)&dat,&size); if (res==ERROR_SUCCESS && dat==1) topmost=true;
    size=sizeof(dat); res=RegQueryValueEx(key,"showui",NULL,&type,(LPBYTE)&dat,&size); if (res==ERROR_SUCCESS && dat==0) showui=false;
    size=sizeof(dat); res=RegQueryValueEx(key,"havehiddenui",NULL,&type,(LPBYTE)&dat,&size); if (res==ERROR_SUCCESS && dat==1) havehiddenui=true;
    RegCloseKey(key);
  }
  if (okay)
  { // we'll check that the window will be visible!
    HRGN rgn=CreateRectRgn(0,0,0,0);
    for (vector<TMonitorInfo>::const_iterator i=monitors.begin(); i!=monitors.end(); i++)
    { HRGN mrgn = CreateRectRgn(i->x,i->y,i->x+i->w,i->y+i->h);
      HRGN r = CreateRectRgn(0,0,0,0);
      CombineRgn(r,rgn,mrgn,RGN_OR);
      DeleteObject(rgn); DeleteObject(mrgn); rgn=r;
    }
    // We've made 'rgn' store the union of all monitors
    bool isin = (RectInRegion(rgn,&rc)==TRUE);
    DeleteObject(rgn);
    okay = isin;
  }
  if (!okay)
  { int cx=GetSystemMetrics(SM_CXSCREEN), cy=GetSystemMetrics(SM_CYSCREEN);
    rc.left=cx/4; rc.top=cy/4; rc.right=cx/2; rc.bottom=cy/2;
  }
  //
  MoveWindow(hwnd,rc.left,rc.top,rc.right-rc.left,rc.bottom-rc.top,FALSE);
  if (topmost) SetWindowPos(hwnd,HWND_TOPMOST,0,0,0,0,SWP_NOSIZE|SWP_NOACTIVATE|SWP_NOCOPYBITS|SWP_NOMOVE|SWP_NOREDRAW);
}


void TViewer::EnsureDirectoryUpToDate()
{ DWORD res = WaitForSingleObject(hdir,0);
  if (res==WAIT_OBJECT_0)
  { ReadDirectory();
    FindNextChangeNotification(hdir);
  }
}

void TViewer::OnRightClick(int mx,int my)
{   
  HMENU hmenu=CreatePopupMenu();
  MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi);
  mi.fMask=MIIM_TYPE|MIIM_ID|MIIM_STATE; mi.fState=MFS_ENABLED;
  mi.fType=MFT_STRING; 
  if (GetAsyncKeyState(VK_CONTROL)<0)
  { mi.wID=103; mi.dwTypeData="Diagnostics"; mi.cch=11; mi.fState=MFS_ENABLED|(showAmp?MFS_CHECKED:0);
    InsertMenuItem(hmenu,0,TRUE,&mi); mi.fState=MFS_ENABLED;
  }
  mi.wID=102; mi.dwTypeData="Download..."; mi.cch=11; InsertMenuItem(hmenu,0,TRUE,&mi);
  mi.wID=101; mi.dwTypeData="Edit..."; mi.cch=7; InsertMenuItem(hmenu,0,TRUE,&mi);
  mi.fType=MFT_SEPARATOR; mi.wID=127; InsertMenuItem(hmenu,0,TRUE,&mi);
  //
  for (int i=pre.size()-1; i>=0; i--)
  { string s = pre[i].desc;
    PopulateMenu(hmenu,s.c_str(),1001+i,(preset==i+1?1:0));
  }
  PopulateMenu(hmenu,"Randomize",1000,(preset==0));
  //
  int cmd=TrackPopupMenu(hmenu,TPM_LEFTALIGN|TPM_TOPALIGN|TPM_RETURNCMD,mx,my,0,hwnd,NULL);
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
  else if (cmd==103) showAmp = (1-showAmp);
}



void TViewer::OnPaint(PAINTSTRUCT *,HDC hdc, int m)
{ DWORD nowtime=0,aftercalc=0,afterdraw=0,afterblt=0; TIMECAPS tc; tc.wPeriodMin=1;
  if (showAmp) {timeGetDevCaps(&tc,sizeof(TIMECAPS));timeBeginPeriod(tc.wPeriodMin);}
  nowtime=timeGetTime();

  DWORD this_period=nowtime-periodtime;
  period[periodi]=this_period;
  periodi++; if (periodi==10) periodi=0;
  periodtime=nowtime;

  if (needsredraw) UpdateBody();
  aftercalc=timeGetTime();
  if (needsredraw) Redraw(-1); needsredraw=false;
  afterdraw=timeGetTime();
  HWND h; if (isfull) h=fullwnds[m]; else h=hwnd;
  RECT r; GetClientRect(h,&r);
  BitBlt(hdc,0,0,r.right,r.bottom,memDCs[m],0,0,SRCCOPY);
  afterblt=timeGetTime();
  if (showAmp)
  { int avperiod=0; for (int i=0; i<10; i++) avperiod+=period[i]; avperiod/=10;
    int fps=0; if (avperiod>0) fps=1000/avperiod;
    banners[0]= "Calc:"+StringInt(aftercalc-nowtime)+"   "
            "Draw:"+StringInt(afterdraw-aftercalc)+"   "
            "Blt:"+StringInt(afterblt-afterdraw)+"  "
            "FPS:"+StringInt(fps);
    bantime=nowtime+3000;
  }
  if (showAmp) timeEndPeriod(tc.wPeriodMin);
}

void TViewer::OnSize(int nw,int nh, int m)
{ HWND h; if (isfull) h=fullwnds[m]; else h=hwnd;
  BITMAP bmp; GetObject(memBMs[m],sizeof(bmp),&bmp);
  if (bmp.bmWidth<nw || bmp.bmHeight<nh)
  { int cx=monitors[m].w, cy=monitors[m].h;
    int width2=nw*3/2, height2=nh*3/2;
    if (width2>cx) width2=cx; if (height2>cy) height2=cy;
    SelectObject(memDCs[m],oldBMs[m]);
    DeleteObject(memBMs[m]);
    HDC hdc=GetDC(0); memBMs[m] = CreateCompatibleBitmap(hdc,width2,height2); ReleaseDC(0,hdc);
    oldBMs[m] = (HBITMAP)SelectObject(memDCs[m],memBMs[m]);
  }
  Redraw(m);
  InvalidateRect(h,NULL,TRUE);
}



void TViewer::OnTick(TWinampVisModule *mod)
{ DWORD nowtime=GetTickCount();
  if (nowtime<calctime) return;
  calctime = nowtime + 1000/bodies[0]->fps;
  if (nowtime>bantime)
  { for (int nb=0; nb<nbodies; nb++) banners[nb]="";
  }

  GotSoundYet=true;
  Calc(mod,nowtime); // that will update the l[],r[],k[] arrays
  needsredraw=true;         // that means that, after WM_PAINT, the body updated and the backbuffer redrawn
  if (isfull) {for (vector<HWND>::const_iterator i=fullwnds.begin(); i!=fullwnds.end(); i++) InvalidateRect(*i,NULL,TRUE);}
  else InvalidateRect(hwnd,NULL,TRUE);
}




// Redraw merely updates the back-buffer. It doesn't flip, and it doesn't recalc.
void TViewer::Redraw(int mon)
{ int m0=mon, m1=mon+1; if (mon==-1) {m0=0;m1=nbodies;}
  for (int m=m0; m<m1; m++)
  { // we assume the membitmap is the right size
    // and we assume the calculations are up to date
    HDC hdc=memDCs[m]; RECT rc; HWND h; if (isfull) h=fullwnds[m]; else h=hwnd; GetClientRect(h,&rc);
    //
    Draw(hdc,rc,bodies[m],banners[m]);
    //
    HGDIOBJ holdp = SelectObject(hdc,GetStockObject(WHITE_PEN));
    HGDIOBJ holdb = SelectObject(hdc,GetStockObject(BLACK_BRUSH));
    HPEN hpb = CreatePen(PS_SOLID,0,RGB(64,64,64)); SelectObject(hdc,hpb);
    int width=rc.right-rc.left, height=rc.bottom-rc.top;
    if (!isfull)
    { MoveToEx(hdc,0,0,NULL); LineTo(hdc,width-1,0); LineTo(hdc,width-1,height-1);
      LineTo(hdc,0,height-1); LineTo(hdc,0,0);
    }
    SelectObject(hdc,GetStockObject(WHITE_PEN)); DeleteObject(hpb);
    //
    if (!isfull && showui)
    { int iconw=GetSystemMetrics(SM_CXVSCROLL), bw=iconw/2, bo=iconw/2, bg=iconw/3;
      // The close box at top right: 
      MoveToEx(hdc,width-bo-bw,bo,NULL); LineTo(hdc,width-bo+1,bo+bw+1);
      MoveToEx(hdc,width-bo-bw,bo+bw,NULL); LineTo(hdc,width-bo+1,bo-1);
      // the full-screen box
      int bx=width-bo-bw-bg-bw, by=bo;
      MoveToEx(hdc,bx,by,NULL); LineTo(hdc,bx+bw,by); LineTo(hdc,bx+bw,by+bw);
      LineTo(hdc,bx,by+bw); LineTo(hdc,bx,by);
      // the right arrow
      bx=width-bo-bw-bg-bw-bg-bw; by=bo;
      MoveToEx(hdc,bx,by,NULL); LineTo(hdc,bx+bw,by+bw/2);
      LineTo(hdc,bx,by+bw); LineTo(hdc,bx,by);
      // the left arrow
      bx=width-bo-bw-bg-bw-bg-bw-bg-bw; by=bo;
      MoveToEx(hdc,bx+bw,by,NULL); LineTo(hdc,bx,by+bw/2);
      LineTo(hdc,bx+bw,by+bw); LineTo(hdc,bx+bw,by);
      // The system menu at top left:
      bx=bo; by=bo;
      MoveToEx(hdc,bx,by+bw/2,NULL); LineTo(hdc,bx+bw,by+bw/2);
    }
 
    SelectObject(hdc,holdp);
    SelectObject(hdc,holdb);
    //
  }
}






void TViewer::StickyInit()
{ preset = 0;
  showAmp = 0;

  bantime=0; curboddesc0="";
  prevtime=0; calctime=0;  cmul=0;
  ZeroMemory(period,sizeof(period)); periodi=0; periodtime=0;
  TBody *bod0=new TBody();
  bodies.push_back(bod0); nbodies=1;
  banners.push_back("");
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
  OnNewMedia("","",""); SelectBody();
}

void TViewer::StickyEnd()
{ for (int i=0; i<nbodies; i++) {if (bodies[i]!=0) delete bodies[i]; bodies[i]=0;}
  nbodies=0; bodies.clear();
  save_recents(&recents);
}


void TViewer::OnNewMedia(const string artist, const string title, const string filename)
{ if (recents.size()==0) load_recents(&recents);
  curregexp="";
  if (artist!="" || title!="" || filename!="")
  { load_rules(&rules,false);
    curregexp = match_rule(&rules,artist,title,filename);
    if (curregexp=="") add_recent(&recents,artist,title,filename,"");
  }
  
  if (preset==0) {EnsureDirectoryUpToDate(); if (GotSoundYet) SelectBody();}
  else
  { banners[0]=title;
    bantime = GetTickCount()+3000;
  }
}





void TViewer::ReadDirectory()
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

void TViewer::SelectBody(bool leave0,bool showrand)
{ int minb=0; if (leave0) minb++;
  for (int nb=minb; nb<nbodies; nb++)
  { int i;
    if (preset!=0) i=preset-1;
    else i=ChooseRandomBody(curregexp,pre,(nb==0?curboddesc0:""));
    if (nb==0) curboddesc0 = pre[i].desc;
    char err[1000];
    bool res=OpenFile(&bodies[nb],pre[i].path.c_str(),err);
    if (res) banners[nb]=ExtractFileName(pre[i].desc); else banners[nb]=err;
    if (res && showrand && preset==0) banners[nb]="Randomize";
  }
  bantime=GetTickCount()+3000;
}



void TViewer::StartFull()
{ if (isfull) return; 
  isfull=true;
  needsredraw=true; // since full-screen is drawn without UI borders/icons
  WNDCLASSEX wcex; ZeroMemory(&wcex,sizeof(wcex)); wcex.cbSize = sizeof(WNDCLASSEX);
  BOOL res=GetClassInfoEx(hInstance,"StickyFullClass",&wcex);
  if (!res)
  { wcex.style = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc = (WNDPROC)FullWndProc;
    wcex.hInstance = hInstance;
    wcex.lpszClassName = "StickyFullClass";
    ATOM res=RegisterClassEx(&wcex);
    if (res==0) {isfull=false;banners[0]="Failed to register full-screen class"; bantime=GetTickCount()+3000; return;}
  }
  TFullWndDat dat; dat.swnd=this; bool okay=true; HWND h;  
  banners.clear(); fullwnds.clear();
  for (int nb=1; nb<nbodies; nb++) delete bodies[nb];
  //
  nbodies=(int)monitors.size();
  for (int nb=0; nb<nbodies; nb++)
  { if (nb!=0) bodies.push_back(new TBody());
    banners.push_back(""); fullwnds.push_back(0);
  }
  SelectBody(true);
  for (int nb=0; nb<nbodies; nb++)
  { dat.nb=nb; char c[100]; wsprintf(c,"StickyFull%i",nb);
    TMonitorInfo &mi=monitors[nb];
    h=CreateWindowEx(WS_EX_TOPMOST,"StickyFullClass",c,WS_POPUP|WS_VISIBLE,mi.x,mi.y,mi.w,mi.h,NULL,NULL,hInstance,&dat);
    fullwnds[nb]=h; okay &= (h!=NULL);
  }
  if (!okay)
  { banners.resize(1); banners[0]="Failed to create full-screen window"; bantime=GetTickCount()+3000;
    for (int nb=0; nb<nbodies; nb++)
    { if (nb!=0) delete bodies[nb];
      DestroyWindow(fullwnds[nb]);
    }
    bodies.resize(1); fullwnds.clear(); isfull=false; nbodies=1;
    return;
  }
}

void TViewer::EndFull()
{ if (!isfull) return;
  isfull=false;
  needsredraw=true;
  for (int nb=0; nb<nbodies; nb++)
  { if (nb!=0) delete bodies[nb];
    HWND h = fullwnds[nb];
    PostMessage(h,WM_CLOSE,0,0);
  }
  bodies.resize(1); fullwnds.clear(); banners.resize(1);
  nbodies=1;
}



void TViewer::Calc(TWinampVisModule *mod, DWORD nowtime)
{ cmul=0;
  if (!GotSoundYet)
  { prevtime=0; for (int p=0; p<6; p++) {l[p]=0.2; r[p]=0.2;}
  } 
  else if (mod!=0)
  { bool adjust=false;
    //if (pLevels->state==play_state)
    { cmul = ((double)(nowtime-prevtime))/10.0; // diff as a multiple of 10ms, the standard interval
      // The max-ratings will fade over time
      if (GotSoundYet)
      { if (cmul>10) // we fade no more frequently than ten times a second.
        { for (int p=0; p<6; p++)
          { max[p]*=0.999; kmax[p]*=0.997;
            if (min[p]<22) min[p]=22; min[p]=min[p]+1*1.002;
            if (kmin[p]<12) kmin[p]=12; kmin[p]*=1.002;
          }
          adjust=true;
          prevtime=nowtime;
        }
      }
      //
      // Winamp doesn't document its frequency ranges. Also, its frequency
      // data is entirely different from that of Windows Media Player.
      // I think that WMP logged it. So I've done the same.
      int boff=3, bwidth=4, bmul=1;
      for (int p=0; p<6; p++) 
      { l[p]=0; r[p]=0; int i;
        for (i=0; i<bwidth; i++)
        { l[p] += bmul*mod->spectrumData[0][boff+p*bwidth+i];
          r[p] += bmul*mod->spectrumData[1][boff+p*bwidth+i];
        }
        l[p] = log(l[p])*300;
        r[p] = log(r[p])*300;
        if (l[p]>max[p]) max[p]=l[p];
        if (r[p]>max[p]) max[p]=r[p];
        if (adjust)
        { if (l[p]<min[p] || r[p]<min[p]) min[p]*=0.9;
        }
      }
    }
    //
    // the waveform is 576 elements big, stored as unsigned chars.
    // 0.vocals=diff, 1.music=average
    int kvoc=0, kmus=0;
    for (int i=0; i<576; i++)
    { int voc = (mod->waveformData[0][i]+mod->waveformData[1][i])/2;
      int mus = (int)(mod->waveformData[0][i])-(int)(mod->waveformData[1][i]);
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
    for (int p=0; p<2; p++)
    { k[p] = (k[p]-kmin[p]) / (kmax[p]-kmin[p]+1); if (k[p]<0) k[p]=0; k[p]*=k[p];
    }
    

    for (int p=0; p<6; p++)
    { if (l[p]>0.01 || r[p]>0.01) {if (!GotSoundYet) prevtime=nowtime; GotSoundYet=true;}
    }
  }
  
}



// given l[] and r[] and k[] and cmul, this function just recalculates the bodies
void TViewer::UpdateBody()
{ double freq[3][6];
  for (int p=0; p<6; p++) {freq[0][p]=l[p]; freq[1][p]=r[p]; freq[2][p]=k[p];}
  //
  for (int nb=0; nb<nbodies; nb++)
  { TBody *body = bodies[nb]; 
    body->AssignFreq(freq,cmul);
    body->RecalcEffects();
    body->Recalc();
  }
}






void TViewer::Draw(HDC hdc,RECT &rc,TBody *body,const string ban)
{ TAmpData ad; ad.l=l; ad.r=r; ad.min=min; ad.max=max; ad.k=k; ad.kmin=kmin; ad.kmax=kmax;
  string err; bool ok=SimpleDraw(hdc,rc,body,ban.c_str(),showAmp?&ad:0);
  if (!ok && banners.size()>0) {banners[0]=err; bantime=GetTickCount()+5000;}
}






bool TViewer::OpenFile(TBody **b,string fn,char *err)
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





void TViewer::RegLoad()
{ HKEY hkey;
  LONG res = RegOpenKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,KEY_READ,&hkey);
  if (res!=ERROR_SUCCESS) return;
  char uf[200],sa[200],pr[200]; *uf=0;*sa=0;*pr=0; DWORD type,size;
  size=200; res=RegQueryValueEx(hkey,"showAmp",0,&type,(LPBYTE)sa,&size); if (res!=ERROR_SUCCESS) *sa=0;
  size=200; res=RegQueryValueEx(hkey,"preset",0,&type,(LPBYTE)pr,&size); if (res!=ERROR_SUCCESS) *pr=0;
  RegCloseKey(hkey);
  int i; int col;
  i=sscanf(sa,"%i",&col); if (i==1) showAmp = col;
  i=sscanf(pr,"%i",&col); if (i==1) preset = col;
}

void TViewer::RegSave()
{ HKEY hkey; DWORD disp;
  LONG res = RegCreateKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,NULL,0,KEY_WRITE,NULL,&hkey,&disp);
  if (res!=ERROR_SUCCESS) return;
  char sa[200], pr[200];
  wsprintf(sa,"%i",(int)showAmp);
  wsprintf(pr,"%i",(int)preset);
  RegSetValueEx(hkey,"showAmp",0,REG_SZ,(LPBYTE)sa,(DWORD)strlen(sa)+1);
  RegSetValueEx(hkey,"preset",0,REG_SZ,(LPBYTE)pr,(DWORD)strlen(pr)+1);
  RegCloseKey(hkey);
}








LRESULT CALLBACK ViewerWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{ TViewer *viewer=(TViewer*)GetWindowLong(hwnd,GWL_USERDATA);
  switch (msg)
  { case WM_CREATE:
    { viewer = new TViewer(hwnd);
      SetWindowLong(hwnd,GWL_USERDATA,(LONG)viewer);
      viewer->oldfn=""; viewer->ofnTimer=SetTimer(hwnd,1,100,NULL);
      viewer->OnCreate();
      return 0;
    }
    case WM_ERASEBKGND: return 0;
    case WM_PAINT:
    { PAINTSTRUCT ps; HDC hdc = BeginPaint(hwnd,&ps);
      viewer->OnPaint(&ps,hdc,0);
      EndPaint(hwnd,&ps); return 0;
    }
    case WM_DESTROY:
    { if (viewer->ofnTimer!=0) KillTimer(hwnd,viewer->ofnTimer); viewer->ofnTimer=0;
      SetWindowLong(hwnd,GWL_USERDATA,0);
      delete viewer;
      PostQuitMessage(0);return 0;
    }
    case WM_TIMER:
    { HWND hwamp = GetParent(hwnd);
      int pos = SendMessage(hwamp,WM_USER,0,125);
      char *cfn = (char*)SendMessage(hwamp,WM_USER,pos,211);
      string newfn(""); if (cfn!=0) newfn=cfn;
      if (newfn!=viewer->oldfn)
      { char *cart = (char*)SendMessage(hwamp,WM_USER,pos,212);
        // tit will be of the form "Unknown artist - title" or "artist - title"
        if (cart==0) cart=""; const char *ctit=strstr(cart," - ");
        string artist; if (ctit==0) artist=cart; else artist=string(cart,ctit-cart);
        string title; if (ctit==0) title=""; else title=ctit+3;
        if (stricmp(artist.c_str(),"unknown artist")==0) artist="";
        if (stricmp(title.c_str(),"untitled")==0) title="";
        viewer->oldfn=newfn;
        viewer->OnNewMedia(artist,title,newfn);
      }
      return 0;
    }
    case WM_KEYDOWN: case WM_KEYUP:
    { HWND hpar=GetParent(hwnd);
      PostMessage(hpar,msg,wParam,lParam);
      return 0;
    }
    case WM_SETCURSOR:
    { int ht=LOWORD(lParam);
      if (ht==HTVSCROLL) {SetCursor(viewer->hcprev);return TRUE;}
      if (ht==HTHSCROLL) {SetCursor(viewer->hcnext);return TRUE;}
      if (ht==HTMAXBUTTON) {SetCursor(viewer->hcfull);return TRUE;}
      if (ht==HTCLOSE) {SetCursor(viewer->hcclose);return TRUE;}
      if (ht==HTSYSMENU) {SetCursor(viewer->hchandle);return TRUE;}
    } break;
    case WM_NCHITTEST:
    { RECT rc; GetWindowRect(hwnd,&rc);
      int x=LOWORD(lParam), y=HIWORD(lParam);
      int iconw=GetSystemMetrics(SM_CXVSCROLL), bw=iconw/2, bo=iconw/2, bg=iconw/3;
      bool left = (x<rc.left+iconw), right = (x>rc.right-iconw);
      bool top = (y<rc.top+iconw), bottom = (y>rc.bottom-iconw);
      bool innerright1 = (x>rc.right-bo-bw*1-bg*0 && x<rc.right-bo-bw*0-bg*0);
      bool innerright2 = (x>rc.right-bo-bw*2-bg*1 && x<rc.right-bo-bw*1-bg*0);
      bool innerright3 = (x>rc.right-bo-bw*3-bg*2 && x<rc.right-bo-bw*2-bg*1);
      bool innerright4 = (x>rc.right-bo-bw*4-bg*3 && x<rc.right-bo-bw*3-bg*2);
      bool innerleft = (x>rc.left+bo && x<rc.left+bo+bw);
      bool innertop = (y>rc.top+bo && y<rc.top+bo+bw);
      if (innerright1 && innertop) return HTCLOSE;
      else if (innerright2 && innertop) return HTMAXBUTTON;
      else if (innerright3 && innertop) return HTHSCROLL; // override this message for "right"
      else if (innerright4 && innertop) return HTVSCROLL; // override this message for "left"
      else if (innerleft && innertop) return HTSYSMENU;
      else if (left && top) return HTTOPLEFT;
      else if (left && bottom) return HTBOTTOMLEFT;
      else if (right && top) return HTTOPRIGHT;
      else if (right && bottom) return HTBOTTOMRIGHT;
      else if (top) return HTTOP;
      else if (bottom) return HTBOTTOM;
      else if (left) return HTLEFT;
      else if (right) return HTRIGHT;
      else return HTCAPTION;
    }
    case WM_NCLBUTTONDBLCLK: {if (wParam==HTCAPTION) {viewer->StartFull(); return 0;}} break;
    case WM_NCLBUTTONDOWN:
    { if (wParam==HTCLOSE) {SendMessage(hwnd,WM_SYSCOMMAND,SC_CLOSE,lParam);return 0;}
      else if (wParam==HTMAXBUTTON) {viewer->StartFull(); return 0;}
      else if (wParam==HTVSCROLL) {viewer->preset--; if (viewer->preset<0) viewer->preset=(int)viewer->pre.size(); viewer->SelectBody(false,true); viewer->RegSave(); return 0;}
      else if (wParam==HTHSCROLL) {viewer->preset++; if (viewer->preset>(int)viewer->pre.size()) viewer->preset=0; viewer->SelectBody(false,true); viewer->RegSave(); return 0;}
      else if (wParam==HTSYSMENU)
      { HMENU hrightmenu=CreatePopupMenu(); MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi);
        mi.fMask=MIIM_DATA|MIIM_ID|MIIM_STATE|MIIM_TYPE; mi.fType=MFT_STRING; mi.fState=MFS_ENABLED; char *c;
        c="Close"; mi.wID=102; mi.dwTypeData=c; mi.cch=strlen(c); InsertMenuItem(hrightmenu,0,TRUE,&mi);
        mi.fState=MFS_ENABLED; if (!viewer->showui) mi.fState|=MFS_CHECKED;
        c="Hide buttons"; mi.wID=103; mi.dwTypeData=c; mi.cch=strlen(c); InsertMenuItem(hrightmenu,0,TRUE,&mi);
        mi.fState=MFS_ENABLED; if (viewer->topmost) mi.fState|=MFS_CHECKED;
        c="Topmost"; mi.wID=101; mi.dwTypeData=c; mi.cch=strlen(c); InsertMenuItem(hrightmenu,0,TRUE,&mi);
        int x=LOWORD(lParam), y=HIWORD(lParam);
        int cmd=TrackPopupMenu(hrightmenu,TPM_LEFTALIGN|TPM_TOPALIGN|TPM_RETURNCMD,x,y,0,hwnd,NULL);
        DestroyMenu(hrightmenu);
        if (cmd==101)
        { viewer->topmost = !viewer->topmost;
          SetWindowPos(hwnd,viewer->topmost?HWND_TOPMOST:HWND_NOTOPMOST,0,0,0,0,SWP_NOSIZE|SWP_NOACTIVATE|SWP_NOMOVE);
        }
        else if (cmd==103)
        { bool okay=true; if (!viewer->havehiddenui && viewer->showui)
          { int res = MessageBox(hwnd,"Do you wish to hide the control buttons in this window?\r\n"
                                      "(They will still be accessible, but invisible. To show them again, "
                                      "you'll have to be able to find this invisible menu button at the top left.",
                                      "Hide Buttons",MB_YESNOCANCEL);
            if (res==IDYES) viewer->havehiddenui=true; else okay=false;
          }
          if (okay)
          { viewer->showui = !viewer->showui; viewer->Redraw(-1); InvalidateRect(hwnd,NULL,TRUE);
          }
        }
        else if (cmd==102) {SendMessage(hwnd,WM_SYSCOMMAND,SC_CLOSE,lParam);return 0;} 
      }
      break;
    }
    case WM_NCRBUTTONDOWN:
    { if (wParam==HTCAPTION) viewer->OnRightClick(LOWORD(lParam),HIWORD(lParam));
      break;
    }
    case WM_SIZE:
    { RECT rc; GetClientRect(hwnd,&rc); 
      viewer->OnSize(rc.right-rc.left,rc.bottom-rc.top,0);
      break;
    }
    case WM_USER:
    { // this comes from the OnRender method of winamp
      TWinampVisModule *mod=(TWinampVisModule*)lParam;
      viewer->OnTick(mod);
      break;
    }
  }
  return DefWindowProc(hwnd,msg,wParam,lParam);
}

LRESULT CALLBACK FullWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{ TFullWndDat *dat;
#pragma warning( push ) // my code is clean! but the compiler won't believe me...
#pragma warning( disable : 4244 4312 )
  if (msg==WM_CREATE)
  { CREATESTRUCT *cs=(CREATESTRUCT*)lParam;
    TFullWndDat *cdat=(TFullWndDat*)cs->lpCreateParams;
    dat = new TFullWndDat; dat->nb=cdat->nb; dat->swnd=cdat->swnd; dat->w=-1; dat->h=-1;
    if (dat->swnd->memBMs[dat->nb]!=0)
    { BITMAP bmp; GetObject(dat->swnd->memBMs[dat->nb],sizeof(bmp),&bmp);
      dat->w=bmp.bmWidth; dat->h=bmp.bmHeight;
    }
    SetWindowLongPtr(hwnd,GWLP_USERDATA,(LONG_PTR)dat);
  }
  else
  { dat=(TFullWndDat*)GetWindowLongPtr(hwnd,GWLP_USERDATA);
    if (msg==WM_DESTROY)
    { delete dat; dat=0; SetWindowLongPtr(hwnd,GWLP_USERDATA,0);
    }
  }
#pragma warning( pop )
  switch (msg)
  { case WM_LBUTTONDOWN: case WM_RBUTTONUP: case WM_KEYDOWN:
    { dat->swnd->EndFull();
    } break;
    case WM_SYSCOMMAND:
    { if (wParam==SC_SCREENSAVE) return 0; // gobble it up! so disabling the screensaver
    } break;
    case WM_ERASEBKGND:
    { return 1;
    }
    case WM_SETCURSOR:
    { SetCursor(NULL); return TRUE;
    } 
    case WM_PAINT:
    { PAINTSTRUCT ps; BeginPaint(hwnd,&ps);
      RECT rc; GetClientRect(hwnd,&rc);
      if (dat->swnd->memDCs[dat->nb]==0)
      { HDC h=GetDC(0);
        dat->swnd->memDCs[dat->nb]=CreateCompatibleDC(h);
        ReleaseDC(0,h);
      }
      if (dat->swnd->memBMs[dat->nb]!=0)
      { if (rc.right-rc.left!=dat->w || rc.bottom-rc.top!=dat->h)
        { SelectObject(dat->swnd->memDCs[dat->nb],dat->swnd->oldBMs[dat->nb]);
          DeleteObject(dat->swnd->memBMs[dat->nb]);
          dat->swnd->memBMs[dat->nb]=0;}
      }
      if (dat->swnd->memBMs[dat->nb]==0)
      { HDC h=GetDC(0); dat->w=rc.right-rc.left; dat->h=rc.bottom-rc.top;
        dat->swnd->memBMs[dat->nb] = CreateCompatibleBitmap(h,dat->w,dat->h);
        ReleaseDC(0,h);
        dat->swnd->oldBMs[dat->nb]=(HBITMAP)SelectObject(dat->swnd->memDCs[dat->nb],dat->swnd->memBMs[dat->nb]);
      }

      DWORD nowtime=0,aftercalc=0,afterdraw=0,afterblt=0; TIMECAPS tc; tc.wPeriodMin=1;
      bool showAmp = (dat->swnd->showAmp>0);
      if (showAmp) {timeGetDevCaps(&tc,sizeof(TIMECAPS));timeBeginPeriod(tc.wPeriodMin);}
      if (showAmp) nowtime=timeGetTime();

      if (showAmp && dat->nb==0) // only count FPS on the primary monitor
      { DWORD this_period=nowtime-dat->swnd->periodtime;
        dat->swnd->period[dat->swnd->periodi]=this_period;
        dat->swnd->periodi++; if (dat->swnd->periodi==10) dat->swnd->periodi=0;
        dat->swnd->periodtime=nowtime;
      }

      if (dat->swnd->needsredraw) dat->swnd->UpdateBody();
      if (showAmp) aftercalc=timeGetTime();
      if (dat->swnd->needsredraw) dat->swnd->Redraw(-1); dat->swnd->needsredraw=false;
      if (showAmp) afterdraw=timeGetTime();
      BitBlt(ps.hdc,0,0,dat->w,dat->h,dat->swnd->memDCs[dat->nb],0,0,SRCCOPY);
      if (showAmp) afterblt=timeGetTime();
      if (showAmp)
      { dat->swnd->banners[dat->nb]= "("+StringInt(dat->nb)+") "
                "Calc:"+StringInt(aftercalc-nowtime)+"   "
                "Draw:"+StringInt(afterdraw-aftercalc)+"   "
                "Blt:"+StringInt(afterblt-afterdraw);
        if (dat->nb==0)
        { int avperiod=0; for (int i=0; i<10; i++) avperiod+=dat->swnd->period[i]; avperiod/=10;
          int fps=0; if (avperiod>0) fps=1000/avperiod;
          dat->swnd->banners[dat->nb] += "   FPS:"+StringInt(fps);
        }
        dat->swnd->bantime=nowtime+3000;
      }
      if (showAmp) timeEndPeriod(tc.wPeriodMin);
      EndPaint(hwnd,&ps);
    } break;
  }
  return DefWindowProc(hwnd,msg,wParam,lParam);
}




// ENUM-MONITOR-CALLBACK is part of module init
BOOL CALLBACK EnumMonitorCallback(HMONITOR,HDC,LPRECT rc,LPARAM)
{ TMonitorInfo mi;
  mi.x=rc->left; mi.y=rc->top; mi.w=rc->right-rc->left; mi.h=rc->bottom-rc->top;
  monitors.push_back(mi);
  return TRUE;
}


int ModuleInit(TWinampVisModule *mod)
{ srand(GetTickCount());
  monitors.clear();
  #ifndef SM_CMONITORS
  DECLARE_HANDLE(HMONITOR);
  #define SM_CMONITORS            80
  #endif
  int num_monitors=GetSystemMetrics(SM_CMONITORS);
  if (num_monitors>1)
  { typedef BOOL (CALLBACK *LUMONITORENUMPROC)(HMONITOR,HDC,LPRECT,LPARAM);
    typedef BOOL (WINAPI *LUENUMDISPLAYMONITORS)(HDC,LPCRECT,LUMONITORENUMPROC,LPARAM);
    HINSTANCE husr=LoadLibrary("user32.dll");
    LUENUMDISPLAYMONITORS pEnumDisplayMonitors=0;
    if (husr!=NULL) pEnumDisplayMonitors=(LUENUMDISPLAYMONITORS)GetProcAddress(husr,"EnumDisplayMonitors");
    if (pEnumDisplayMonitors!=NULL) (*pEnumDisplayMonitors)(NULL,NULL,EnumMonitorCallback,NULL);
    if (husr!=NULL) {FreeLibrary(husr); husr=NULL;}
  }
  if (monitors.size()==0)
  { TMonitorInfo mi; mi.x=0; mi.y=0;
    mi.w=GetSystemMetrics(SM_CXSCREEN); mi.h=GetSystemMetrics(SM_CYSCREEN);
    monitors.push_back(mi);
  }
  WNDCLASS wc; ZeroMemory(&wc,sizeof(wc));
  BOOL res=GetClassInfo(hInstance,"Sticky2WindowClass",&wc);
  if (!res)
  { wc.style = CS_HREDRAW|CS_VREDRAW|CS_DBLCLKS;
    wc.lpfnWndProc = ViewerWndProc;
    wc.hInstance = hInstance;
    wc.lpszClassName = "Sticky2WindowClass";
    res=RegisterClass(&wc);
    if (!res) {MessageBox(mod->hwndParent,"Reg Error","Error",MB_OK);return 1;}
  }
  //
  HWND hwnd=CreateWindowEx(WS_EX_TOOLWINDOW,"Sticky2WindowClass","Sticky 2",
    WS_POPUP,0,0,0,0,mod->hwndParent,NULL,hInstance,0);
  if (hwnd==NULL) {MessageBox(mod->hwndParent,"Create Error","Error",MB_OK);return 1;}
  ShowWindow(hwnd,SW_SHOWNORMAL);
  mod->userData=hwnd;
  //
  return 0;
}

int ModuleRender(TWinampVisModule *mod)
{ HWND hwnd=(HWND)mod->userData;
  SendMessage(hwnd,WM_USER,0,(LPARAM)mod);
  return 0;
}


void ModuleQuit(TWinampVisModule *mod)
{ HWND hwnd=(HWND)mod->userData;
  DestroyWindow(hwnd);
}



// ------------------------------------------------------------------------------------------
// ------------------------------------------------------------------------------------------
// ------------------------------------------------------------------------------------------


int ModuleConfig(TWinampVisModule *mod) {return MessageBox(mod->hwndParent,"No configurable options","Dancing Stick Figures",MB_OK);}
TWinampVisModule const_mod = {"Dancing Stick Figures", NULL, NULL, 0, 0, 50, 50, 2, 2, {0,}, {0,}, ModuleConfig, ModuleInit, ModuleRender, ModuleQuit};
TWinampVisModule *getModule(int which) {if (which==0) return &const_mod; else return NULL;}
TWinampVisHeader const_hdr = {0x100, "Dancing Stick Figures", getModule };
extern "C" __declspec(dllexport) TWinampVisHeader* winampVisGetHeader() {return &const_hdr;}
BOOL APIENTRY DllMain(HINSTANCE h,DWORD,LPVOID) {hInstance=h; return TRUE;}










