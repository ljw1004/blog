#define WIN32_LEAN_AND_MEAN
#include <string>
#include <vector>
#include <list>
using namespace std;
#include <math.h>
#include <windows.h> 
#include <shellapi.h>
#pragma warning( push )
#pragma warning( disable : 4201 )
#include <mmsystem.h>
#pragma warning( pop )
#pragma warning( push ) // some of the winamp headers are a bit poor...
#pragma warning( disable : 4311 4312 4100 4127 )
#include "wamp-sdk/studio/wac.h"
#include "wamp-sdk/studio/corecb.h"
#include "wamp-sdk/bfc/wndcreator.h"
#include "wamp-sdk/bfc/appcmds.h"
#include "wamp-sdk/common/xlatstr.h"
#include "wamp-sdk/common/popup.h"
#include "wamp-sdk/common/corehandle.h"
#include "wamp-sdk/common/guiobjwnd.h"
#pragma warning( pop )
#include "../body.h"
#include "../utils.h"
using namespace stk;
//
typedef struct {unsigned char spectrumData[2][576]; char waveformData[2][576];} VisData;
LRESULT CALLBACK FullWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam); // for the full-screen window
class StickyWnd; typedef struct {StickyWnd *swnd; int nb; HDC hmemdc; HBITMAP hmembm,holdbm; int w,h;} TFullWndDat;
typedef struct {int x,y,w,h;} TMonitorInfo; vector<TMonitorInfo> monitors;

static const GUID stickyguid = {0x9c029b91,0x6bee,0x49d8,{0xa4,0xb,0x81,0x51,0xd8,0x90,0xb7,0x42}}; // {9C029B91-6BEE-49d8-A40B-8151D890B742}
extern WAComponentClient *the;


list<string> recents;  // the recent music we have heard




class StickyWnd : public GuiObjectWnd, CoreCallbackI, AppCmdsI
{ public:
  StickyWnd() : GuiObjectWnd(), isfull(false), nbodies(0) {setName("Sticky");} // set titlebar text for window
  virtual ~StickyWnd()
  { if (isfull) EndFull();
    api->core_delCallback(0,this);
    if (hdir!=0) FindCloseChangeNotification(hdir); hdir=0;
    StickyEnd();

  }
  // Window creation services:
  static const char *getWindowTypeName() {return "Dancing Stick Figures";} // thinger's statusbar hint
  static GUID getWindowTypeGuid() {return stickyguid;}
  static void setIconBitmaps(ButtonWnd *button) {button->setBitmaps(the->gethInstance(),2,NULL,1,1);} 
  // Window behaviour: 
  bool isfull; vector<HWND> fullwnds;
  int onInit()
  { int retval=GuiObjectWnd::onInit();
    StickyInit(); setTimer(0x0B0B0D0D,9);
    api->core_addCallback(0,this);
    appcmds_deleteAll();
    appcmds_addCmd("Full screen", 1, AppCmds::SIDE_LEFT); 
    appcmds_addCmd("Prev", 2, AppCmds::SIDE_RIGHT);
    appcmds_addCmd("Next", 3, AppCmds::SIDE_RIGHT);
    getGuiObject()->guiobject_addAppCmds(this); 
    return retval;
  }
  int corecb_onTitleChange(const char *title)
  { CoreHandle *core = new CoreHandle("Main");
    if (core->getCurrent()==NULL)
    { mtitle=title;
      martist="";
      mfilename="";
    }
    else
    { char info[4096];
      api->metadb_getMetaData(core->getCurrent(), MT_TITLE, info, 4096, MDT_STRINGZ); info[4095]=0;
      mtitle=info;
      api->metadb_getMetaData(core->getCurrent(), MT_ARTIST, info, 4096, MDT_STRINGZ); info[4095]=0;
      martist=info;
      api->metadb_getMetaData(core->getCurrent(), MT_PLAYSTRING, info, 4096, MDT_STRINGZ); info[4095]=0;
      mfilename=info;
    }
    delete core;
    NewMedia(martist,mtitle,mfilename);
    return 0;
  }
  
  void timerCallback(int id)
  { if (id!=0x0B0B0D0D) {GuiObjectWnd::timerCallback(id);return;}
    //
    DWORD nowtime=GetTickCount();
    if (nowtime<calctime) return;
    calctime = nowtime + 1000/bodies[0]->fps;
    if (nowtime>bantime)
    { for (int nb=0; nb<nbodies; nb++) banners[nb]="";
    }
    //
    VisData myVisData;
    int retval = api->core_getVisData(0, &myVisData, sizeof(VisData));
    GotSoundYet = (retval!=0);
    Calc(&myVisData,nowtime);
    needsredraw=true;
    if (isfull) {for (vector<HWND>::const_iterator i=fullwnds.begin(); i!=fullwnds.end(); i++) InvalidateRect(*i,NULL,FALSE);}
    else invalidate();
  }
  int onPaint(Canvas *canvas)
  { PaintBltCanvas pbc; if (canvas==NULL) {pbc.beginPaint(this); canvas=&pbc;}
    GuiObjectWnd::onPaint(canvas);
    DWORD nowtime=0,aftercalc=0,afterdraw=0; TIMECAPS tc; tc.wPeriodMin=1;
    if (showAmp) {timeGetDevCaps(&tc,sizeof(TIMECAPS));timeBeginPeriod(tc.wPeriodMin);}
    nowtime = timeGetTime();
    //
    DWORD this_period=nowtime-periodtime;
    period[periodi]=this_period;
    periodi++; if (periodi==10) periodi=0;
    periodtime=nowtime;
    //
    if (needsredraw) {UpdateBody(); needsredraw=false;}
    aftercalc = timeGetTime();
    RECT crect = clientRect();
    Draw(canvas->getHDC(),crect,bodies[0],banners[0]);
    afterdraw = timeGetTime();
    if (showAmp)
    { int avperiod=0; for (int i=0; i<10; i++) avperiod+=period[i]; avperiod/=10;
      int fps=0; if (avperiod>0) fps=1000/avperiod;
      banners[0]= "Calc:"+StringInt(aftercalc-nowtime)+"   "
                  "Draw:"+StringInt(afterdraw-aftercalc)+"   "
                  "FPS:"+StringInt(fps);
      bantime=nowtime+3000;
    }
    if (showAmp) timeEndPeriod(tc.wPeriodMin);
    return 1;
  }
  int onRightButtonDown(int x,int y);
  void appcmds_onCommand(int id, const RECT *buttonRect)
  { AppCmdsI::appcmds_onCommand(id,buttonRect);
    if (id==2) {preset--; if (preset<0) preset=(int)pre.size(); SelectBody(false,true); RegSave();}
    else if (id==3) {preset++; if (preset>(int)pre.size()) preset=0; SelectBody(false,true); RegSave();}
    else if (id==1 && !isfull) StartFull();
  }
  void StartFull()
  { if (isfull) return; 
    isfull=true;
    WNDCLASSEX wcex; ZeroMemory(&wcex,sizeof(wcex)); wcex.cbSize = sizeof(WNDCLASSEX);
    BOOL res=GetClassInfoEx(gethInstance(),"StickyFullClass",&wcex);
    if (!res)
    { wcex.style = CS_HREDRAW | CS_VREDRAW;
      wcex.lpfnWndProc = (WNDPROC)FullWndProc;
      wcex.cbClsExtra = 0;
      wcex.cbWndExtra = 0;
      wcex.hInstance = gethInstance();
      wcex.hIcon = NULL;
      wcex.hCursor = NULL;
      wcex.hbrBackground = NULL;
      wcex.lpszMenuName	= NULL;
      wcex.lpszClassName = "StickyFullClass";
      wcex.hIconSm = NULL;
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
      h=CreateWindowEx(WS_EX_TOPMOST,"StickyFullClass",c,WS_POPUP|WS_VISIBLE,mi.x,mi.y,mi.w,mi.h,NULL,NULL,gethInstance(),&dat);
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
  void EndFull()
  { if (!isfull) return;
    isfull=false;
    for (int nb=0; nb<nbodies; nb++)
    { if (nb!=0) delete bodies[nb];
      HWND h = fullwnds[nb];
      PostMessage(h,WM_CLOSE,0,0);
    }
    bodies.resize(1); fullwnds.clear(); banners.resize(1);
    nbodies=1;
  }
  // Sticky behaviour
  void StickyInit(); // sets up timings, "body" pointer, &c.
  void StickyEnd();
  void NewMedia(const string artist, const string title, const string filename);
  void RegLoad();  // sticky stores its colour+timing+other preferences
  void RegSave();  // in the registry
  void SelectBody(bool leave0=false,bool showrand=false); string curboddesc0;     // loads the body based on the "preset" variable
  bool OpenFile(TBody **b,string fn,char *err); // loads the file into the body
  void Calc(VisData *dat,DWORD nowtime); // updates l[],r[] arrays
  void UpdateBody();  // given l[],r[] arrays, recalculates limb positions
  void Draw(HDC hdc,RECT &rc,TBody *body,const string banner); 
  void ReadDirectory();  // updates the internal list according to the contents of the stick directory
  void EnsureDirectoryUpToDate();
  // sticky properties
  string martist, mtitle, mfilename, curregexp; // set by MediaInfo/NotifyNewMedia
  list<TRule> rules;      // rules for selecting which stick to show
  int showAmp;            // should we show amplitude sticks?
  int preset;             // index of current stick
  vector<TPre> pre; // all of the presets: their display-names, and full paths. Populated in finalconstruct.
  vector<string> banners; unsigned int bantime; // a text banner, plus when we stop displaying it.
  double l[6],r[6],k[6]; // we divide the spectrum up into 6 bands 
  double cmul;
  bool needsredraw;  // a tick from Winamp merely sets l[6],r[6],k[6],needsredraw and calls InvalidateRect.
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


StickyWnd *debugw=0;
void stk::luassertmsg(const char *s,const char *msg,const char *f,unsigned int l)
{ if (debugw==0) return; if (debugw->banners.size()==0) return;
  char c[2000]; wsprintf(c,"%s '%s' - %s:%u",msg,s,f,l);
  debugw->banners[0]=c; debugw->bantime=GetTickCount()+3000;
}


LRESULT CALLBACK FullWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{ TFullWndDat *dat;
#pragma warning( push ) // my code is clean! but the compiler won't believe me...
#pragma warning( disable : 4244 4312 )
  if (msg==WM_CREATE)
  { CREATESTRUCT *cs=(CREATESTRUCT*)lParam;
    TFullWndDat *cdat=(TFullWndDat*)cs->lpCreateParams;
    dat = new TFullWndDat; dat->nb=cdat->nb; dat->swnd=cdat->swnd; dat->hmemdc=0; dat->hmembm=0; dat->holdbm=0; dat->w=0; dat->h=0;
    SetWindowLongPtr(hwnd,GWLP_USERDATA,(LONG_PTR)dat);
  }
  else
  { dat=(TFullWndDat*)GetWindowLongPtr(hwnd,GWLP_USERDATA);
    if (msg==WM_DESTROY)
    { if (dat->hmembm!=0) {SelectObject(dat->hmemdc,dat->holdbm); DeleteObject(dat->hmembm); dat->hmembm=0;}
      if (dat->hmemdc!=0) DeleteDC(dat->hmemdc); dat->hmemdc=0;
      delete dat; dat=0; SetWindowLongPtr(hwnd,GWLP_USERDATA,0);
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
    } break;
    case WM_SETCURSOR:
    { SetCursor(NULL); return TRUE;
    } break;
    case WM_PAINT:
    { PAINTSTRUCT ps; BeginPaint(hwnd,&ps);
      RECT rc; GetClientRect(hwnd,&rc);
      if (dat->hmemdc==0)
      { HDC h=GetDC(0);
        dat->hmemdc=CreateCompatibleDC(h);
        ReleaseDC(0,h);
      }
      if (dat->hmembm!=0)
      { if (rc.right-rc.left!=dat->w || rc.bottom-rc.top!=dat->h) {SelectObject(dat->hmemdc,dat->holdbm); DeleteObject(dat->hmembm); dat->hmembm=0;}
      }
      if (dat->hmembm==0)
      { HDC h=GetDC(0); dat->w=rc.right-rc.left; dat->h=rc.bottom-rc.top;
        dat->hmembm = CreateCompatibleBitmap(h,dat->w,dat->h);
        ReleaseDC(0,h);
        dat->holdbm=(HBITMAP)SelectObject(dat->hmemdc,dat->hmembm);
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

      if (dat->swnd->needsredraw) dat->swnd->UpdateBody(); dat->swnd->needsredraw=false;
      if (showAmp) aftercalc=timeGetTime();
      dat->swnd->Draw(dat->hmemdc,rc, dat->swnd->bodies[dat->nb], dat->swnd->banners[dat->nb]);
      if (showAmp) afterdraw=timeGetTime();
      BitBlt(ps.hdc,0,0,dat->w,dat->h,dat->hmemdc,0,0,SRCCOPY);
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


void StickyWnd::StickyInit()
{ preset = 0;
  showAmp = 0;

  bantime=0; curboddesc0="";
  prevtime=0; calctime=0; cmul=0;
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
  NewMedia("","",""); SelectBody();
}

void StickyWnd::StickyEnd()
{ for (int i=0; i<nbodies; i++) {if (bodies[i]!=0) delete bodies[i]; bodies[i]=0;}
  nbodies=0; bodies.clear();
  save_recents(&recents);
}


void StickyWnd::EnsureDirectoryUpToDate()
{ DWORD res = WaitForSingleObject(hdir,0);
  if (res==WAIT_OBJECT_0)
  { ReadDirectory();
    FindNextChangeNotification(hdir);
  }
}

void StickyWnd::NewMedia(const string artist, const string title, const string filename)
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



void StickyWnd::ReadDirectory()
{ pre.clear();
  // first we add any files
  DirScan(pre,GetStickDir()+"\\");
  HideRedundancy(pre);
  // next we add resources if they're not already there
  if (pre.size()==0)
  { for (unsigned int i=1; ; i++)
    { char c[MAX_PATH]; int res=LoadString(the->gethInstance(),i+1000,c,MAX_PATH);
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


// We will pre-build our list using this TIntMenu type.
class TIntMenu
{ public: TIntMenu(string as,int aid) : s(as), id(aid) {};
  ~TIntMenu()
  { for (list<TIntMenu*>::const_iterator i=children.begin(); i!=children.end(); i++)
    { TIntMenu *m=*i; delete m;
    }
  }
  string s; int id; list<TIntMenu*> children;
  void Insert(const string as,int aid)
  { const char *c=as.c_str(), *subpos=strchr(c,'\\'); 
    if (subpos==NULL)
    { TIntMenu *m=new TIntMenu(as,aid); children.push_back(m);
      return;
    }
    else
    { string ps(c,subpos-c), cs(subpos+1);
      for (list<TIntMenu*>::const_iterator i=children.begin(); i!=children.end(); i++)
      { TIntMenu *m=*i;
         if (StringLower(m->s)==StringLower(ps)) {m->Insert(cs,aid); return;}
      }
      TIntMenu *m=new TIntMenu(ps,0); children.push_back(m); m->Insert(cs,aid);
    }
  }
};


PopupMenu *RecMakeMenu(TIntMenu *imen,PopupMenu *menu,int sid)
{ if (imen->children.size()==0)
  { menu->addCommand(imen->s.c_str(), imen->id, (sid==imen->id?1:0));
    return NULL;
  }
  PopupMenu *submen = new PopupMenu();
  for (list<TIntMenu*>::const_iterator i=imen->children.begin(); i!=imen->children.end(); i++)
  { RecMakeMenu(*i,submen,sid);
  }
  if (menu==NULL) return submen; // at the top level, just return it
  menu->addSubMenu(submen,imen->s.c_str()); return NULL;
}

int StickyWnd::onRightButtonDown(int x,int y)
{ GuiObjectWnd::onRightButtonDown(x,y);
  DWORD res = WaitForSingleObject(hdir,0);
  if (res==WAIT_OBJECT_0)
  { ReadDirectory();
    FindNextChangeNotification(hdir);
  }
  TIntMenu imen("",0);
  imen.Insert("Randomize",1000);
  for (int i=0; i<(int)pre.size(); i++)
  { string s = pre[i].desc;
    imen.Insert(s,1001+i);
  }
  // that will have created a proper hierarchical menu. Now we turn it into reality  
  PopupMenu *menu = RecMakeMenu(&imen,NULL,1000+preset);
  menu->addSeparator(); menu->addCommand("Edit...", 2); menu->addCommand("Download...",3);
  if (GetAsyncKeyState(VK_CONTROL)<0) menu->addCommand("Diagnostics",4,showAmp);
  //
  int command = menu->popAtMouse();
  delete menu;
  //
  if (command>=1000) {preset=command-1000; SelectBody(); RegSave(); return 1;}
  else if (command==2)
  { save_recents(&recents);
    ShellExecute(NULL,"open",GetStickDir().c_str(),NULL,NULL,SW_SHOWNORMAL);
  }
  else if (command==3) ShellExecute(NULL,"open","http://www.wischik.com/lu/senses/sticky/extrasticks.html",NULL,NULL,SW_SHOWNORMAL);
  else if (command==4) {showAmp = 1-showAmp; RegSave();}
  return 1;
}


void StickyWnd::SelectBody(bool leave0,bool showrand)
{ int minb=0; if (leave0) minb++;
  for (int nb=minb; nb<nbodies; nb++)
  { int i;
    if (preset!=0) i=preset-1;
    else i=ChooseRandomBody(curregexp, pre,(nb==0?curboddesc0:""));
    if (nb==0) curboddesc0 = pre[i].desc;
    char err[1000];
    bool res=OpenFile(&bodies[nb],pre[i].path.c_str(),err);
    if (res) banners[nb]=ExtractFileName(pre[i].desc); else banners[nb]=err;
    if (res && showrand && preset==0) banners[nb]="Randomize";
  }
  bantime=GetTickCount()+3000;
}



void StickyWnd::Calc(VisData *dat, DWORD nowtime)
{ cmul=0;
  if (!GotSoundYet)
  { prevtime=0; for (int p=0; p<6; p++) {l[p]=0.2; r[p]=0.2;}
  } 
  else
  { bool adjust=false;
    //if (pLevels->state==play_state)
    { // The max-ratings will fade over time
      cmul = ((double)(nowtime-prevtime))/10.0; // diff as a multiple of 10ms, the standard interval
      if (GotSoundYet)
      { if (cmul>10) // we fade no more frequently than ten times a second.
        { for (int p=0; p<6; p++)
          { max[p]*=0.999;
            if (min[p]<22) min[p]=22; min[p]=min[p]+1*1.002;
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
        { l[p] += bmul*dat->spectrumData[0][boff+p*bwidth+i];
          r[p] += bmul*dat->spectrumData[1][boff+p*bwidth+i];
        }
        l[p] = log(l[p])*300;
        r[p] = log(r[p])*300;
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
      { int voc = (dat->waveformData[0][i]+dat->waveformData[1][i])/2;
        int mus = (int)(dat->waveformData[0][i])-(int)(dat->waveformData[1][i]);
        kvoc += voc*voc; kmus += mus*mus;
      }
      kvoc=(int)sqrt(kvoc); kmus=2*(int)sqrt(kmus);
      k[0]=kvoc; k[1]=kmus;
      if (k[0]<kmin[0]) kmin[0]=k[0];  if (k[0]>kmax[0]) kmax[0]=k[0];
      if (k[1]<kmin[1]) kmin[1]=k[1];  if (k[1]>kmax[1]) kmax[1]=k[1];
    }
    for (int p=0; p<6; p++)
    { l[p] = (l[p]-min[p]) / (max[p]-min[p]+1); if (l[p]<0) l[p]=0; l[p]*=l[p];
      r[p] = (r[p]-min[p]) / (max[p]-min[p]+1); if (r[p]<0) r[p]=0; r[p]*=r[p];
    }
    for (p=0; p<6; p++)
    { if (l[p]>0.01 || r[p]>0.01) {if (!GotSoundYet) prevtime=nowtime; GotSoundYet=true;}
    }


    for (int p=0; p<2; p++)
    { k[p] = (k[p]-kmin[p]) / (kmax[p]-kmin[p]+1); if (k[p]<0) k[p]=0; k[p]*=k[p];
    }

  }
  
}


// given l[] and r[], this function just recalculates the bodies
void StickyWnd::UpdateBody()
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



void StickyWnd::Draw(HDC hdc,RECT &rc,TBody *body,const string ban)
{ TAmpData ad; ad.l=l; ad.r=r; ad.min=min; ad.max=max; ad.k=k; ad.kmin=kmin; ad.kmax=kmax;
  string err; bool ok=SimpleDraw(hdc,rc,body,ban.c_str(),showAmp?&ad:0);
  if (!ok && banners.size()>0) {banners[0]=err; bantime=GetTickCount()+5000;}
}






bool StickyWnd::OpenFile(TBody **b,string fn,char *err)
{ if (fn=="") return false;
  if (fn[0]!='*') return LoadBody(b,fn.c_str(),err,lbForUse);
  int id=atoi(fn.substr(1).c_str());
  HRSRC hrsrc = FindResource(the->gethInstance(),MAKEINTRESOURCE(id),RT_RCDATA);
  DWORD size = SizeofResource(the->gethInstance(),hrsrc);
  HGLOBAL hglob = LoadResource(the->gethInstance(),hrsrc);
  char *rs = (char*)LockResource(hglob);
  // NB. The ReadData call is destructive. Therefore we must take
  // a copy of the memory block.
  char *buf = new char[size+1];
  memcpy(buf,rs,size); buf[size]=0;
  bool res=(*b)->ReadData(buf,err,rdOverwrite,NULL);
  delete[] buf;
  return res;
}





void StickyWnd::RegLoad()
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

void StickyWnd::RegSave()
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



// ENUM-MONITOR-CALLBACK is part of static WAC creation
BOOL CALLBACK EnumMonitorCallback(HMONITOR,HDC,LPRECT rc,LPARAM)
{ TMonitorInfo mi;
  mi.x=rc->left; mi.y=rc->top; mi.w=rc->right-rc->left; mi.h=rc->bottom-rc->top;
  monitors.push_back(mi);
  return TRUE;
}


class WACSticky : public WAComponentClient {
public:
  WACSticky() : WAComponentClient("Dancing Stick Figures")
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
    registerService(new WndCreateCreatorSingle< CreateWndByGuid<StickyWnd> >); // make a window
    registerService(new WndCreateCreatorSingle< CreateBucketItem<StickyWnd> >); // have it listed in the thinger
    registerAutoPopup(getGUID(), getName()); // have it listed in the main context menu
  }
  virtual ~WACSticky() {}
  virtual GUID getGUID() {return stickyguid;}
  virtual void onCreate() {} 
  virtual void onDestroy() {}
private:
};
static WACSticky wac; WAComponentClient *the=&wac;





