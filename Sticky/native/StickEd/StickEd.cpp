#include <windows.h>
#include <wtypes.h>
#include <shellapi.h>
#include <commdlg.h>
#include <commctrl.h>
#include <mmsystem.h>
#pragma warning( push )
#pragma warning( disable: 4786 4702 )
#include <string>
#include <vector>
#include <list>
#include <map>
#include <algorithm>
using namespace std;
#pragma warning( pop )
#include <math.h>
#include <stdlib.h>
#include <stdio.h>
#pragma warning( push )
#pragma warning( disable: 4201 )
#include <vfw.h>
#pragma warning( pop )
#include "avi_utils.h"
#include "../zip.h"
#include "../unzip.h"
#include "../regex.h"
#pragma hdrstop // precompiled headers end here
#include "../body.h"
#include "../utils.h"
#include "resource.h"
//---------------------------------------------------------------------------
#ifdef __BORLANDC__
#include <float.h>
#include <condefs.h>
USEUNIT("..\regex.cpp");
USEUNIT("..\body.cpp");
USEUNIT("..\utils.cpp");
USEUNIT("..\unzip.cpp");
USEUNIT("avi_utils.cpp");
USERC("edres.rc");
USEUNIT("..\zip.cpp");
//---------------------------------------------------------------------------
#pragma warn -8080
#endif
//---------------------------------------------------------------------------
#include <sys/stat.h>
using namespace stk;




HINSTANCE hInstance;
UINT StickClipboardFormat;
unsigned char *sampledat[3]={0,0,0}; unsigned int samplesize[3];
HWND hwnd_acc=0; HACCEL hac;

COLORREF CREF(stk::RGBCOLOR rgb)
{
	return RGB(rgb.r, rgb.g, rgb.b);
}

void stk::luassertmsg(const char *s,const char *msg,const char *f,unsigned int l)
{ char c[2000]; wsprintf(c,"%s '%s'\r\n%s : %u",msg,s,f,l);
  MessageBox(hwnd_acc,c,"Assertion failed",MB_OK);
}



string reggarble(const string s,bool emptyiswildcard=false);
double myatan2(double dy,double dx) {if (dy==0) {if (dx<0) return pi; else return 0;} else return atan2(dy,dx);}
int CCW(POINT p0,POINT p1,POINT p2) {return ((p1.x-p0.x)*(p2.y-p0.y) > (p1.y-p0.y)*(p2.x-p0.x)) ? 1 : -1;}
bool intersects(POINT p1, POINT p2, POINT p3, POINT p4) {return (((CCW(p1,p2,p3)*CCW(p1,p2,p4))<=0) && ((CCW(p3,p4,p1)*CCW(p3,p4,p2)<=0)));}
bool inpoly(POINT *pts, int npts, POINT p, RECT &brc) // assumes a closed poly
{ if (p.x<brc.left || p.x>brc.right || p.y<brc.top || p.y>brc.bottom) return false;
  POINT poutside=p; poutside.x=brc.right+50; int nints=0;
  for (int i=0; i<npts-1; i++) {if (intersects(p,poutside, pts[i],pts[i+1])) nints++;}
  return (nints&1);
}
void PumpMessages()
{ MSG msg; for (;;)
  { BOOL res=PeekMessage(&msg,0,0,0,PM_REMOVE);
    if (!res||msg.message==WM_QUIT) return;
    if (!TranslateAccelerator(hwnd_acc,hac,&msg))
    { TranslateMessage(&msg); DispatchMessage(&msg);
    }
  }
}
string StringAng(double a) {string s=""; if (a<0) {a=-a; s="-";} char c[20]; sprintf(c,"%.1lf",a*180.0/pi); return s+c+"deg";}
string StringLength(double m) {char c[20]; sprintf(c,"%.2lf",m); return string(c)+"cm";}
string StringFrac(double f) {char c[20]; sprintf(c,"%.2lf",f); return string(c)+"%";}
string StringNiceFloat(double d)
{ char c[100]; sprintf(c,"%.3lf",d); int len=strlen(c);
  if (len>1 && c[len-1]=='0') {c[len-1]=0; len--;}
  if (len>1 && c[len-1]=='0') {c[len-1]=0; len--;}
  if (len>1 && c[len-1]=='0') {c[len-1]=0; len--;}
  if (len>1 && c[len-1]=='.') {c[len-1]=0;}
  return string(c);
}


//
const int ArmLength=20;
const int ArmSpotLength = 3;
const int PieLength=10;
const int SelPieLength=20;
const int SpotLength=4;
const int PlusLength=8;
const int PlusOffLength=5;
const int SelThickLength=2;
const int AnchorLength=20;
const int AnchorOffLength=6;
const int AnchorMargin=10;

// ------------------------------------------------------------------------------------------
// ------------------------------------------------------------------------------------------
// ------------------------------------------------------------------------------------------

// -1=htMiss 0=spot, 1=plus, 2=amin, 3=amax, 4=line, 5=root, 6=pie, 7=shape
enum THitType {htMiss,htSpot,htPlus,htAmin,htAmax,htLine,htRoot,htPie,htShape,htAroot};
// 0=none 1=create, 2=edit, 3=zoom
enum TTool {tNone,tCreate,tEdit,tZoom};
// 0=nothing, 1=button down on a node, 2=dragging to create new line, 3=moving an existing node
// 4=dragging aoff, 5=dragging ascale, 6=animating, 7=zoom-rubberbanding, ...
enum TMode {mNothing,mNodePressing,mCreateDragging,mNodeMoving,mAoffDragging,
            mAscaleDragging,mLminDragging,mLmaxDragging,mNothingTesting,mZoomDragging,
            mSelDragging,
            mShapeLinePressing,mShapeSpotPressing,mCreateCornerDragging,mCornerMoving,mExporting};

enum TSetSubject {tsLine,tsFill,tsLineFill,tsFreq,tsWhatever};

typedef struct {int r,g,b;} COL; int ncols=7;
COL defcols[7]={{0,0,0}, {255,255,255}, {255,20,10},{30,245,20},{30,20,225},{200,200,10},{200,10,200}};
enum TColIndex {cBlack, cWhite, cRed, cGreen, cBlue, cYellow, cPurple};

typedef struct {COL c; string name;} TUserColour; list<TUserColour> usercols; 
typedef struct {double r; bool reflect; string name;} TUserCum; list<TUserCum> usercums;
list<double> userthick;
bool UserSnap, UserGrid, WhitePrint;
bool ShowAngles, ShowJoints, ShowInvisibles;
COLORREF UserBmpBackground; bool UserCheckUpright;
//
string exportfn; // name of the .avi
bool exportproc; // export a procession? or just the current stick?
string exportfigs;// the procession
int exporttune;  // 0=dancing 1=thrashing 2=to a WAV file
string exportwav;// the wav file
int exportw,exporth; // dimensions of exported video
bool exporttimefixed;// is it a fixed time? or the duration of the wav file?
int exporttime;  // in milliseconds.
int exportproctime; // milliseconds each figure is on stage
int exportfps;   // frames per second
bool exportcompress; DWORD exportopt; // show the compression dialog? and its fourCC, or 0 if empty
vector<TPre> exportpre; // just an internal cache we use
//
typedef struct {string fn,back;} TUserBack; list<TUserBack> UserBacks;
void RegLoadUser(); void RegSaveUser();
int ParseUserCols(const char *); unsigned int WriteUserCols(char *buf,unsigned int buflen);
int ParseUserCums(const char *); unsigned int WriteUserCums(char *buf,unsigned int buflen);
int ParseUserThicks(const char*);unsigned int WriteUserThicks(char *buf,unsigned int buflen);
int ParseUserBacks(const char*); unsigned int WriteUserBacks(char *buf,unsigned int buflen);






typedef struct
{ int s; // shape index. Either this will be to a real shape, or to the "limbs" shape, or -1
  int n; // limb index. For a limb, this will be its index. Or -1.
  THitType type; // limbs use all fields here, shapes use only htLine or htShape or htSpot, misses are htMiss
  int si; double sf; // for a shape/htLine, user clicked on the line starting at index si, fraction sf of the way through
} THitTest;

typedef struct
{ double screenang, screenrad;
  double relang, relrad;
  double f;
  bool isin;
} TAdjAngle;

class TUndoData
{ public:
  TUndoData() : buf(0), len(0), sbuf(0), slen(0) {}
  void releasebmps() {for (list<TBmp>::iterator bi=bmps.begin(); bi!=bmps.end(); bi++) bi->release(); bmps.clear();}
  char *buf; unsigned int len;
  char *sbuf; unsigned int slen;
  list<TBmp> bmps;
  list<int> sellimbs, selshapes;
};
// An UndoData may be hanging around in the the free-stack, or the used-stack.
// In both cases, its 'buf' is kept around. That's so we can just reuse
// some already-allocated memory.
// However, the bmps array is only used for certain undo-points, and must
// be empty in other undo-points and on the free-stack. The bmps array contains
// its own private buffes, and all its hbms are 0.


typedef struct {double min[6], max[6], l[6], r[6];} TSpot;
// a TSpot is a sample that we get from analyzing a wav file, and fourier-transforming
// it, in preparation for exporting sticks to AVI that are dancing to the wav


class TEditor
{ public:
  TEditor();
  ~TEditor();
  bool isready;
  bool ExportCancelled,ExportTerminated;  // reset at start of FileExport. Is set by various cancel buttons.
  HWND mhwnd,chwnd; HWND hstatwnd; 
  HBITMAP hbackdrop; string fnbackdrop;
  TBody *body;
  TLimb rubber; // where the creating- or dragging-rubber band is
  TJointRef rubend; // -1 if the rubber band ends in mid-air, i if it ends on a spot (i.e. creating a shape)
  int cori;  // if we have corner-creating rubber bands, it comes on hit.s.p[] between cori,cori+1
  double corx,cory; // corx,y are the body-coords of the target of the rubber band
  TJointRef cortarget,corotarget; // cortarget is the spot index where we've landed. Otarget is for dragging, where it was before we dragged it. Cori=-1,cortarget=-1 for none.
  
  TMode mode; THitTest hit; // this hit is the thing that's currently selected
  list<int> sellimbs, selshapes; bool numsel; // additionally, for multiple selection, I've added these lists
  void IncNextChan(); int nextchan, nextband; // these are the channel and band for the next one to be created
  string nextlinestyle,nextfillstyle,nextfreqstyle;
  TTool tool;
  int zoomx0,zoomy0,zoomx,zoomy;
  // the zoomcords and time are used to draw rubberbanding both for zoom-in and for multi-select
  THitTest LastHover, ThisHover; bool IsHoverTimer; unsigned int EndHoverTimer;

  void Redraw() {InvalidateRect(chwnd,NULL,FALSE);}
  //{HDC hdc=GetDC(chwnd); Draw(hdc); ReleaseDC(chwnd,hdc);}
  HDC memDC; HBITMAP memBM,oldBM; int width,height; // (temp) the hdc-coordinates where we'll drawing
  void Size(int width,int height);
  void Recalc(int n);

  bool Draw(HDC hdc,string *err=0);
  void DrawPie(HDC hdc,TLimb &dst,int length,bool fillall);
  void DrawPiePart(HDC hdc,int x0,int y0,double length,double ang0,double ang,double ang1);
  void DrawLinePart(HDC hdc,double thick,double ang,int x0,int y0,int x1,int y1,int x2,int y2);
  double zoom, offx, offy; // screenpos = (point+off)*scale+midwin
  void Scale(double x,double y, int *sx,int *sy);
  void Unscale(int x,int y, double *sx,double *sy);
  int b2x(double x); int b2y(double y);
  double x2b(int x); double y2b(int y);
  
  int circlelimb_from_shape(int shape); // if this shape is a circle-only fill, return circle index, otherwise -1.
  int circleshape_from_limb(int limb);
  THitTest PointAtCoords(int x,int y,bool isrightclick);
  void ObjectsInRect(double x0,double y0,double x1,double y1,list<int> &limbs,list<int> &shapes);
  void ShowHitTest();
  TAdjAngle AngleFromOff(double dx,double dy,double ang0,double aoff,double ascale);
  TAdjAngle AngleNearbyFromOff(double dx,double dy,double ang0,double ascreenprev);
  double SnapAngle(double ang,bool clampzero);
  bool SnapToGrid(int *x,int *y);

  void ButtonDown(int x,int y);
  void SelectWobble(int x,int y); // when the button is down to select and the mouse has moved marginally
  void SelectShapeLineWobble(int x,int y); // similarly, but when the button was down on a shape's line
  void SelectShapeSpotWobble(int x,int y); // similarly, but when the button was donw on a shape's spot
  void CreatingMove(int x,int y); // when we're rubber-banding and the mouse moves
  void CornerMove(int x,int y); // when we're rubber-banding a new or existing corner and the mouse moves
  void RepositioningMove(int i,int x,int y,bool suppresscalcdraw=false); // when we're moving an existing node
  void AngleMove(int x,int y); // when we're moving an angle handle
  void SpringMove(int x,int y); // when we're moving a spring handle
  void RubberMove(int x,int y); // when we're rubber-banding
  void Unselect(); // when the mouse lets go after selecting a thing
  void FinishCreate(); // when the mouse lets go after drag-creating a thing
  void FinishCreateCorner(); // same, but for creating a corner
  void FinishRepositioning(); // when the mouse lets go after repositioning a node
  void FinishAngling(); // when we finish angling
  void FinishSpringing(); // when we finish springing
  void FinishZooming(); // when we finish rubberband-zooming
  void FinishSeling(); // when we finish rubberband-selecting
  void RightClick(int x,int y);
  void ShapeClick(int x,int y,THitTest &ht); // for right-clicking on a shape
  void BackgroundClick(int x,int y);
  void HoverMonitor(int x,int y);

  void LazyCreateCircleShapes(); // create fills inside circles if an op was applied to them implied
  void Backdrop(const string fn,bool suppressredraw=false);
  void DeleteLimbOrShape(); void InsertLimb();
  bool Copy(); void Paste(); void Cut();
  void Flip();
  void Stretch(int dir); void RecStretch(int n,double scale);
  void SetChanBand(int c,int b);
  void SetCum(double c,int reflect); 
  void SetAnchor(int anch);
  void ToggleLineType(); void SetVisibility(TSetSubject subj,int newv);
  void ToggleNegative(); void SetAlternate(int newa);
  void SetCol(TSetSubject subj,TColIndex i); void SetCol(TSetSubject subj,int r,int g,int b);
  void SetThickness(double w);
  void SetEffect(TSetSubject subj,const string effect);
  void SetStyle(TSetSubject subj, int index);
  void SetStyle(char shortcutkey);
  void SetF(int node, int chan,int band, double f,bool suppresscalcdraw=false);
  void SetOrder(int dir);
  void SetBitmap(const string bn);
  void Cursor(int dir);
  void Zoom(int dir);
  void GetRootLimbsOfSellimbs(list<int> *roots); // a collection of all the roots from limbs
  bool GetRootLimbsOfSelection(list<int> *roots); // from all shapes


  void SetScrollBars();
  void Scroll(bool horiz,int code,int pos);
  double s_bleft,s_bright,s_btop,s_bbottom; // we keep these, to re-use them (only) while
  double s_fleft,s_fright,s_ftop,s_fbottom; // the user does a thumb-track

  void UserColours(HWND hpar); void UserThicknesses(HWND hpar); void Bitmaps(HWND hpar);
  void UserCumulatives(HWND hpar);
  string AddBitmap(const char *fn);
  void DeleteBitmap(const string zname);
  string RenameBitmap(const string oldzn,const string newzn); 
  string CleanBitmapName(const string s);
  void Effects(HWND hpar);
  string CleanEffectName(const string s);
  string EffectClick(HWND hpar,const string s);
  void Styles(HWND hpar);
  string CleanStyleName(const string s);
  string StyleClick(HWND hpar,const string s);
  void ApplyStyle(const string name,bool suppressredraw=false);
  TStyle StyleFromName(const string name);
  void Category();
  void TargetFramerate();


  void EditUndo(); void EditRedo(); void MarkUndo(bool withbitmaps=false);
  list<TUndoData> undos; int undopos; // we normally sit at '0', the front of the undos list
  list<TUndoData> frees; // free buffers for storing undo positions
  void DebugPaint(HDC hdc,RECT &rc);
  void FileOpen(char *fn); 
  bool FileSave(); 
  bool FileSaveAs(char *fn); 
  void FileExport(); int fft(WavChunk *wav, TSpot *spots, int spotsize);
  void FileNew(); void FileReady();
  bool FileClose();
  bool MaybeCheckUpright(bool forcedialog=false); void FixUpright();
  char curfile[MAX_PATH]; bool ismodified;
  void ShowTitle(); void ShowHint(string s); void SetCursor(); void UpdateMenus();
  HCURSOR hCursor[8]; // 1=move 2=absrel 3=angle 4=smallcreate 5=bigcreate 6=zoom 7=pointer

  void Invert();
  void Animate();
  void Tick(bool atend);
  void StopAnimate();
  int idTimer;
  unsigned int timeStart, timePrev;
  int samplei;
};






TEditor::TEditor()
{ isready=false;
  chwnd=0; mhwnd=0;
  memDC=CreateCompatibleDC(NULL);
  memBM = CreateCompatibleBitmap(memDC,100,100);
  oldBM = (HBITMAP)SelectObject(memDC,memBM);
  hbackdrop=0; fnbackdrop="";
  idTimer=0;
  tool=tCreate;
  ismodified=false; 
  hit.s=-1; hit.n=-1; hit.type=htMiss; rubend.i=-1; cori=-1; sellimbs.clear(); selshapes.clear();
  LastHover.n=-1; LastHover.type=htMiss; ThisHover.n=-1; ThisHover.type=htMiss; IsHoverTimer=false;
  hCursor[1]=LoadCursor(hInstance,MAKEINTRESOURCE(IDC_MOVE));
  hCursor[2]=LoadCursor(hInstance,MAKEINTRESOURCE(IDC_ABSREL));
  hCursor[3]=LoadCursor(hInstance,MAKEINTRESOURCE(IDC_ANGLE));
  hCursor[4]=LoadCursor(hInstance,MAKEINTRESOURCE(IDC_SMALLCREATE));
  hCursor[5]=LoadCursor(hInstance,MAKEINTRESOURCE(IDC_BIGCREATE));
  hCursor[6]=LoadCursor(hInstance,MAKEINTRESOURCE(IDC_ZOOM));
  hCursor[7]=LoadCursor(hInstance,MAKEINTRESOURCE(IDC_POINTER));
  body=new TBody();
  undos.clear(); undopos=-1; 
  //
}

TEditor::~TEditor()
{ SelectObject(memDC,oldBM);
  DeleteObject(memDC); memDC=NULL;
  DeleteObject(memBM); memBM=NULL;
  if (hbackdrop!=0) DeleteObject(hbackdrop); hbackdrop=0; fnbackdrop="";
  if (body!=NULL) delete body; body=NULL;
  for (list<TUndoData>::iterator i=undos.begin(); i!=undos.end(); i++)
  { if (i->buf!=0) delete[] i->buf; i->buf=0;
    if (i->sbuf!=0) delete[] i->sbuf; i->sbuf=0;
    i->releasebmps();
  }
  undos.clear();
  for (list<TUndoData>::iterator i=frees.begin(); i!=frees.end(); i++)
  { if (i->buf!=0) delete[] i->buf; i->buf=0;
    if (i->sbuf!=0) delete[] i->sbuf; i->sbuf=0;
    LUASSERT(i->bmps.size()==0);
  }
  frees.clear();
  undopos=-1;
}


void TEditor::EditUndo()
{ int newundopos=undopos+1;
  TUndoData ud; ud.buf=0; int i=0;
  if (undopos==-1)
  { if (undos.size()>0) ud=undos.front();
  }
  else
  { for (list<TUndoData>::const_iterator it=undos.begin(); it!=undos.end() && i<=newundopos+1; it++,i++)
    { ud=*it;
    }
  }
  if (ud.buf==0) return; // trying to undo but we don't have a history that far back
  // if we weren't already in an undo, we have to save ourselves
  // Moreover, if the top of the undostack had different bitmaps, we have to save our
  // own bitmaps.
  if (undopos==-1)
  { MarkUndo(ud.bmps.size()>0);
  }
  undopos=newundopos;
  //
  char err[1000];
  body->ReadData(ud.buf,err,rdOverwrite,NULL);
  //
  StylesFromString(ud.sbuf,body->styles);
  //
  if (ud.bmps.size()>0)
  { for (vector<TBmp>::iterator i=body->bmps.begin(); i!=body->bmps.end(); i++) i->release();
    body->bmps.clear();
    for (list<TBmp>::const_iterator i=ud.bmps.begin(); i!=ud.bmps.end(); i++)
    { TBmp b; b.name=i->name; b.bufsize=i->bufsize; b.buf=new char[b.bufsize]; memcpy(b.buf,i->buf,b.bufsize);
      PrepareBitmapData(&b,true); body->bmps.push_back(b);
    }
  }
  body->MakeChildren();
  MakeBindexes(body); MakeEindexes(body);
  sellimbs=ud.sellimbs; selshapes=ud.selshapes; hit.n=-1; hit.s=-1; hit.type=htMiss;
  Recalc(0);
  Redraw(); SetCursor();
  UpdateMenus();
}

void TEditor::EditRedo()
{ if (undopos==-1) return;
  undopos--;
  TUndoData ud; ud.buf=0; int i=0;
  // undopos=-1: front
  // undopos=0: first
  for (list<TUndoData>::const_iterator it=undos.begin(); it!=undos.end() && i<=undopos+1; it++,i++)
  { ud=*it;
  }
  if (ud.buf==0) return;
  char err[1000];
  body->ReadData(ud.buf,err,rdOverwrite,NULL);
  //
  StylesFromString(ud.sbuf,body->styles);
  //
  sellimbs=ud.sellimbs; selshapes=ud.selshapes; hit.n=-1; hit.s=-1; hit.type=htMiss;
  if (ud.bmps.size()>0)
  { for (vector<TBmp>::iterator i=body->bmps.begin(); i!=body->bmps.end(); i++) i->release();
    body->bmps.clear();
    for (list<TBmp>::const_iterator i=ud.bmps.begin(); i!=ud.bmps.end(); i++)
    { TBmp b; b.name=i->name; b.bufsize=i->bufsize; b.buf=new char[b.bufsize]; memcpy(b.buf,i->buf,b.bufsize);
      PrepareBitmapData(&b,true); body->bmps.push_back(b);
    }
  }
  body->MakeChildren();
  MakeBindexes(body); MakeEindexes(body);
  Recalc(0);
  Redraw(); SetCursor();
  if (undopos==-1) {TUndoData ud=undos.front(); undos.pop_front(); ud.releasebmps(); frees.push_back(ud);}
  // tricky, that line just above!
  // the undo queue doesn't store the current position (unless we're inside an undo-redo detour)
  // so, when we finish that detour, we have to remove it.
  UpdateMenus();
}

void TEditor::MarkUndo(bool withbitmaps)
{ // first, if there were any 'redos' pending, we'll forget them
  bool inmiddle=false;
  while (undopos>-1)
  { inmiddle=true;
    TUndoData ud=undos.front(); undos.pop_front(); ud.releasebmps(); frees.push_back(ud); undopos--;
  }
  if (inmiddle) {TUndoData ud=undos.front(); undos.pop_front(); ud.releasebmps(); frees.push_back(ud);}
  // next, if we'd reached the limit, forget the oldest one
  while (undos.size()>=5)
  { TUndoData ud=undos.back(); undos.pop_back(); ud.releasebmps(); frees.push_back(ud);
  }
  // now, either reuse an old buffer, or create a new one.
  TUndoData ud; ud.buf=0; if (frees.size()>0) {ud=frees.back(); frees.pop_back();}
  LUASSERT(ud.bmps.size()==0);
  if (ud.buf==0) ud.buf=new char[10000]; ud.len=10000; 
  if (ud.sbuf==0) ud.sbuf=new char[1000]; ud.slen=1000;
  ud.sellimbs=sellimbs; ud.selshapes=selshapes;
  // save our current state in there
  unsigned int wlen = body->WriteData(ud.buf,ud.len,0);
  if (wlen>ud.len)
  { delete[] ud.buf; ud.buf = new char[wlen*2]; ud.len=wlen*2;
    body->WriteData(ud.buf,ud.len,0);
  }
  //
  unsigned int slen = StylesToString(ud.sbuf,ud.slen,body->styles);
  if (slen>ud.slen) {delete[] ud.sbuf; ud.slen=slen*2; ud.sbuf=new char[ud.slen]; StylesToString(ud.sbuf,ud.slen,body->styles);}
  //
  if (withbitmaps)
  { for (vector<TBmp>::const_iterator bi=body->bmps.begin(); bi!=body->bmps.end(); bi++)
    { TBmp b; b.name=bi->name; b.bufsize=bi->bufsize;
      b.buf=new char[b.bufsize]; memcpy(b.buf,bi->buf,b.bufsize);
      ud.bmps.push_back(b);
    }
  }
  undos.push_front(ud);
  UpdateMenus();
}


int dbp(HDC hdc,int y,int,const string s)
{ TextOut(hdc,0,y,s.c_str(),s.length());
  return y+16;
}

void TEditor::DebugPaint(HDC hdc,RECT &rc)
{ int y=0; int w=rc.right-rc.left; int c=0;
  if (undopos==-1) y=dbp(hdc,y,w,"*");
  for (list<TUndoData>::const_iterator i=undos.begin(); i!=undos.end(); i++,c++)
  { unsigned char *buf = (unsigned char*)i->buf;
    string s=""; s.resize(20);
    for (int j=0; j<20; j++)
    { unsigned char c=buf[j];
      if (c>=32 && c<=127) s[j]=c; else if (c==0) s[j]='0'; else s[j]='.';
    }
    s[19]=0;
    y=dbp(hdc,y,w,s);
    if (undopos==c) y=dbp(hdc,y,w,"*");
  }
}




int TEditor::circlelimb_from_shape(int shape)
{ stk::TShape &s = body->shapes[shape];
  if (s.p.size()!=2) return -1;
  if (s.p[0].i!=s.p[1].i) return -1;
  if (s.p[0].ar==s.p[1].ar) return -1;
  int i=s.p[0].i;
  if (body->limbs[i].type!=3) return -1;
  return i;
}
int TEditor::circleshape_from_limb(int nlimb)
{ TLimb &limb = body->limbs[nlimb];
  if (limb.type!=3) return -1;
  for (int i=0; i<(int)body->shapes.size(); i++)
  { stk::TShape &shape = body->shapes[i];
    if (shape.p.size()==2 && shape.p[0].i==nlimb && shape.p[1].i==nlimb && shape.p[0].ar!=shape.p[1].ar) return i;
  }
  return -1;
}


// We assume that the x,y given to us are relative to the origin (i.e. to limb 0)
void TEditor::Scale(double x,double y,int *sx,int *sy)
{ x+=offx; y+=offy;
  x*=zoom; y*=zoom;
  *sx=width/2+(int)x; *sy=height/2+(int)y;
}
int TEditor::b2x(double x) {x+=offx; if (mode==mNothingTesting) x+=body->anchx; x*=zoom; return width/2+(int)x;}
int TEditor::b2y(double y) {y+=offy; if (mode==mNothingTesting) y+=body->anchy; y*=zoom; return height/2+(int)y;}
int gb2x(void *dat,double x) {TEditor *e=(TEditor*)(dat); if (e==0) return 0; return e->b2x(x);}
int gb2y(void *dat,double y) {TEditor *e=(TEditor*)(dat); if (e==0) return 0; return e->b2y(y);}


void TEditor::Unscale(int sx,int sy, double *x,double *y)
{ sx=sx-width/2; sy=sy-height/2;
  *x=((double)sx)/zoom-offx; *y=((double)sy)/zoom-offy;
}
double TEditor::x2b(int x)
{ double d=x-width/2;
  d/=zoom;
  d-=offx;
  if (mode==mNothingTesting) d-=body->anchx;
  return d;
}
double TEditor::y2b(int y) {double d=y-height/2; d/=zoom; d-=offy; if (mode==mNothingTesting) d-=body->anchy; return d;}

// Given dx,dy, and a line whose calculated aoff is ang0 (allows us to calculate parent's bias)
// and whose parameters are aoff and ascale, figure out the angle to this point
// Returns whether or not is inside.
TAdjAngle TEditor::AngleFromOff(double dx,double dy,double ang0,double aoff,double ascale)
{ TAdjAngle aa;
  double bias=ang0-aoff;
  double ang=myatan2(dy,dx); aa.screenang=ang;
  double r=sqrt((double)(dx*dx+dy*dy)); aa.screenrad=r*zoom;
  ang=ang-bias; // now ang is in the same league as aoff and ascale.
  // Next: if ascale was negative, we'll put ang negative of aoff;
  // if ascale was positive, we'll put ang positive of aoff.
  if (ascale<0) {while (ang<aoff) ang+=2*pi; while (ang>aoff) ang-=2*pi;}
  else if (ascale>0) {while (ang>aoff) ang-=2*pi; while (ang<aoff) ang+=2*pi;}
  else // if ascale was zero then we're doing something different: finding the angle that's close to what it was before
  {
  }
  aa.relang=ang;
  aa.relrad=r;
  // The question now is merely: is the angle inside the range?
  if (ascale==0) {aa.f=0; aa.isin=true;}
  else {aa.f=(ang-aoff)/ascale; aa.isin = (aa.f>=0 && aa.f<=1);}
  //
  return aa;
}

TAdjAngle TEditor::AngleNearbyFromOff(double dx,double dy,double ang0,double ascreenprev)
{ TAdjAngle aa;
  if (ascreenprev<-4*pi || ascreenprev>4*pi) ascreenprev=0;
  double bias=ang0;
  double r=sqrt((double)(dx*dx+dy*dy)); aa.screenrad=r*zoom;
  aa.relrad=r;
  // We want to find the angle that's closes to ascreenprev
  double aprev=ascreenprev-bias;
  double ang=myatan2(dy,dx); ang=ang-bias;
  if (ang<-100 || ang>100) ang=0; // !!!
  while (ang>aprev+pi) ang-=2*pi; while (ang<aprev-pi) ang+=2*pi;
  while (ang>2*pi) ang-=2*pi; while (ang<-2*pi) ang+=2*pi;
  aa.relang=ang; aa.screenang=ang+bias;
  aa.f=1; aa.isin = true;
  return aa;
}






void TEditor::ShowHitTest()
{ RECT rc; GetClientRect(chwnd,&rc);
  HDC hdc=GetDC(chwnd);
  for (int y=rc.top; y<rc.bottom; y++)
  { for (int x=rc.left; x<rc.right; x++)
    { THitTest t = PointAtCoords(x,y,false);
      if (t.type!=htMiss && t.s==hit.s)
      { COLORREF c;
        if (t.type==htShape) c=RGB(255,0,0);
        else if (t.type==htAroot) c=RGB(0,255,0);
        else if (t.type==htSpot) c=RGB(0,0,255);
        else {int i=t.si*31; i=i%255; c=RGB(i,i,i);}
        SetPixel(hdc,x,y,c);
      }
    }
  }
  ReleaseDC(chwnd,hdc);
}

void TEditor::ObjectsInRect(double x0,double y0,double x1,double y1,list<int> &limbs,list<int> &shapes)
{ limbs.clear(); shapes.clear();
  for (int sh=(int)body->shapes.size()-1; sh>=0; sh--)
  { stk::TShape &shape = body->shapes[sh];
    bool simplecircle = (!shape.limbs && shape.p.size()==2 && shape.p[0].i==shape.p[1].i && shape.p[0].ar!=shape.p[1].ar && body->limbs[shape.p[0].i].type==3);
    if (simplecircle)
    { TLimb &limb = body->limbs[shape.p[0].i];
      double dx=limb.x-limb.x0,dy=limb.y-limb.y0, rad=sqrt(dx*dx+dy*dy);
      if (limb.x0-rad>=x0 && limb.x0+rad<=x1 && limb.y0-rad>=y0 && limb.y0+rad<=y1)
      { shapes.push_back(sh);
        int li = circlelimb_from_shape(sh); LUASSERT(li!=-1);
        limbs.push_back(li);
      }
    }  
    if (!shape.limbs && shape.p.size()>0 && !simplecircle) // for a real shape...
    { // we'll only check its vertices
      bool isin=true;
      for (int si=0; si<(int)shape.p.size() && isin; si++)
      { TJointRef j0=shape.p[si];
        double x,y; jointpos(&x,&y,body->limbs[j0.i],j0.ar);
        if (x<x0 || x>x1 || y<y0 || y>y1) isin=false;
      }
      if (isin) shapes.push_back(sh);
    }
    //
    if (shape.limbs)
    { for (int i=1; i<body->nlimbs; i++)
      { TLimb &limb=body->limbs[i];
        bool isin=true, circle=false;
        if (limb.type==0 || limb.type==2) // a line
        { if (limb.x<x0 || limb.x>x1 || limb.y<y0 || limb.y>y1) isin=false;
          if (limb.x0<x0 || limb.x0>x1 || limb.y0<y0 || limb.y0>y1) isin=false;
        }
        else if (limb.type==1) // an arc
        { if (limb.x<x0 || limb.x>x1 || limb.y<y0 || limb.y>y1) isin=false;
          double x,y; jointpos(&x,&y,limb,true);
          if (x<x0 || x>x1 || y<y0 || y>y1) isin=false;
        }
        else if (limb.type==3) // a circle
        { double dx=limb.x-limb.x0,dy=limb.y-limb.y0, rad=sqrt(dx*dx+dy*dy);
          if (limb.x0-rad<x0 || limb.x0+rad>x1 || limb.y0-rad<y0 || limb.y0+rad>y1) isin=false;
          circle=true;
        }
        if (isin) limbs.push_back(i);
        if (isin && circle) {int si=circleshape_from_limb(i); if (si!=-1) shapes.push_back(si);}
      }
    }
  }
  limbs.sort(); limbs.unique(); shapes.sort(); shapes.unique();
}

THitTest TEditor::PointAtCoords(int sx,int sy,bool isrightclick)
{ THitTest t; t.s=-1; t.sf=0; t.si=-1; t.n=-1; t.type=htMiss;
  int limbs=-1; for (int i=0; i<(int)body->shapes.size() && limbs==-1; i++) {if (body->shapes[i].limbs) limbs=i;}
  // Did we click on a rotation handle, or the pie, for the current selection?
  for (list<int>::const_iterator sli=sellimbs.begin(); sli!=sellimbs.end(); sli++)
  { TLimb &sel=body->limbs[*sli];
    bool showmin=(sel.chan!=4 || sel.band!=0), showmax=showmin;
    if (sel.type==1) showmin=true;      
    if (sel.type==0 || sel.type==1)
    { int a1x=b2x(sel.x0)+(int)(((double)ArmLength)*cos(sel.ang1)), a1y=b2y(sel.y0)+(int)(((double)ArmLength)*sin(sel.ang1));
      int a0x=b2x(sel.x0)+(int)(((double)ArmLength)*cos(sel.ang0)), a0y=b2y(sel.y0)+(int)(((double)ArmLength)*sin(sel.ang0));
      if (showmax && sx>=a1x-ArmSpotLength && sx<a1x+ArmSpotLength && sy>=a1y-ArmSpotLength && sy<a1y+ArmSpotLength) {t.n=*sli; t.s=limbs; t.type=htAmax; return t;}
      if (showmin && sx>=a0x-ArmSpotLength && sx<a0x+ArmSpotLength && sy>=a0y-ArmSpotLength && sy<a0y+ArmSpotLength) {t.n=*sli; t.s=limbs; t.type=htAmin; return t;}
      if (isrightclick) // on a right click we're willing to check if it was a pie click
      { TAdjAngle a=AngleFromOff(x2b(sx)-sel.x0,y2b(sy)-sel.y0,sel.ang0,sel.aoff,sel.ang-sel.ang0);
        if (a.isin && a.screenrad<SelPieLength) {t.n=*sli; t.s=limbs; t.type=htPie; return t;}
      }
    }
  }
  // Did we click on a spring handle for the current selection?
  for (list<int>::const_iterator sli=sellimbs.begin(); sli!=sellimbs.end(); sli++)
  { TLimb &sel=body->limbs[*sli];
    if (sel.type==2 || sel.type==3)
    { bool showmin=(sel.chan!=4 || sel.band!=0), showmax=showmin;
      double ddx=cos(sel.ang+0.5*pi)*ArmLength/2, ddy=sin(sel.ang+0.5*pi)*ArmLength/2; int dx=(int)ddx, dy=(int)ddy;
      int a0x=b2x(sel.x0+sel.lmin*cos(sel.ang))-dx, a0y=b2y(sel.y0+sel.lmin*sin(sel.ang))-dy;
      int a1x=b2x(sel.x0+sel.length*cos(sel.ang))-dx, a1y=b2y(sel.y0+sel.length*sin(sel.ang))-dy;
      if (showmax && sx>=a1x-ArmSpotLength && sx<a1x+ArmSpotLength && sy>=a1y-ArmSpotLength && sy<a1y+ArmSpotLength) {t.n=*sli; t.s=limbs; t.type=htAmax; return t;}
      if (showmin && sx>=a0x-ArmSpotLength && sx<a0x+ArmSpotLength && sy>=a0y-ArmSpotLength && sy<a0y+ArmSpotLength) {t.n=*sli; t.s=limbs; t.type=htAmin; return t;}
    }
  }
  // we check pins of the current shapes before anything else.
  for (list<int>::const_iterator ssi=selshapes.begin(); ssi!=selshapes.end(); ssi++)
  { stk::TShape &shape = body->shapes[*ssi];
    // if the thing is a circle/shape, then don't treat its pins as pins
    int li=circlelimb_from_shape(*ssi);
    if (li==-1)
    { for (int si=0; si<(int)shape.p.size(); si++)
      { TLimb &limb = body->limbs[shape.p[si].i]; bool ar=shape.p[si].ar;
        double x,y; jointpos(&x,&y,limb,ar,0);
        int cx=b2x(x), cy=b2y(y); double dx=sx-cx, dy=sy-cy, dd=dx*dx+dy*dy;
        if (dd<SpotLength*SpotLength*2) {t.s=*ssi; t.si=si; t.sf=0; t.n=-1; t.type=htSpot; return t;}
      }
    }
  }
  // we must check all joints before checking any lines. 
  for (int i=0; i<body->nlimbs; i++)
  { TLimb &limb=body->limbs[i];
    int cx=b2x(limb.x), cy=b2y(limb.y); double dx=sx-cx, dy=sy-cy, dd=dx*dx+dy*dy;
    if (dd<SpotLength*SpotLength*2) {t.n=i; t.s=limbs; t.type=htSpot; return t;}
    if (limb.type==1 || limb.type==3) // also check the endpoint of an arc/circle
    { double x,y; jointpos(&x,&y,limb,true,0);
      cx=b2x(x); cy=b2y(y); dx=sx-cx; dy=sy-cy; dd=dx*dx+dy*dy;
      if (dd<SpotLength*SpotLength*2) {t.n=i; t.s=limbs; t.type=htAroot; return t;}
    }
  }
  // now check the lines and shapes...
  // if there is a currently just-hit shape, its lines will have precedence over lines-as-limbs.
  TEffPt ept;
  int firsts=(int)body->shapes.size()-1; if (hit.s!=-1 && hit.n==-1) firsts++;
  for (int shi=firsts; shi>=0; shi--)
  { int sh=shi; bool checkingsel = (shi==(int)body->shapes.size());
    if (checkingsel) sh=hit.s;
    stk::TShape &shape = body->shapes[sh];
    bool simplecircle = (!shape.limbs && shape.p.size()==2 && shape.p[0].i==shape.p[1].i && shape.p[0].ar!=shape.p[1].ar && body->limbs[shape.p[0].i].type==3);
    if (simplecircle)
    { TLimb &limb = body->limbs[shape.p[0].i];
      double dx=limb.x-limb.x0,dy=limb.y-limb.y0, rad=sqrt(dx*dx+dy*dy);
      double x0=b2x(limb.x0), y0=b2y(limb.y0), sr=b2x(limb.x0+rad)-x0;
      double mr=sqrt((sx-x0)*(sx-x0) + (sy-y0)*(sy-y0));
      double drad = sr-mr;
      if (drad<4 && drad>-4) {t.n=-1; t.s=sh; t.sf=0; t.si=0; t.type=htLine; return t;}
      if (!checkingsel && shape.brush.type!=ctNone && mr<sr) {t.n=-1; t.s=sh; t.sf=0; t.si=-1; t.type=htShape; return t;}
    }  
    if (!shape.limbs && shape.p.size()>0 && !simplecircle) // for a real shape...
    { // first, we'll built the pt[] array for this shape
      int ptpos=0; for (int si=0; si<(int)shape.p.size(); si++)
      { TJointRef j0=shape.p[si], j1=shape.p[(si+1)%shape.p.size()];
        TLimb &limb0=body->limbs[j0.i]; //, &limb1=body->limbs[j1.i];
        if (j0.i==j1.i && j0.ar!=j1.ar && (limb0.type==1 || limb0.type==3)) ptpos=add_arc(gb2x,gb2y,this,&ept,ptpos,limb0,j0.ar,si);
        else ptpos=add_pt(gb2x,gb2y,this,&ept,ptpos,limb0,j0.ar,si);
      }
      if (shape.p.size()>2) ptpos=ept.add(ptpos,ept.pt[0].x,ept.pt[0].y,shape.p.size()-1); // to close it!      
      // Now we have the pt[] array. We check for line-clicks first.
      RECT brc={0,0,0,0}; // we'll also figure out a bounding rectangle
      for (int i=0; i<ptpos; i++)
      { double x0=ept.pt[i].x, y0=ept.pt[i].y;
        double x1=ept.pt[(i+1)%ptpos].x, y1=ept.pt[(i+1)%ptpos].y;
        double x=sx, y=sy;
        x0+=0.0001; y0+=0.0001; // for divide-by-zero problems...
        double m=(y1-y0)/(x1-x0), c=y1-m*x1; // equation of line joining points
        double am=-1/m, ac=sy-am*x; // equation of perpendicular
        double xd=(ac-c)/(m-am), yd=m*xd+c; // coordinates of closest point on line
        double dx=xd-x, dy=yd-y, dd=dx*dx+dy*dy;
        double dh=(x1-x0)*(x1-x0), dv=(y1-y0)*(y1-y0);
        double f; if (dh>dv) f=(xd-x0)/(x1-x0); else f=(yd-y0)/(y1-y0);
        double dmax; if (shi==(int)body->shapes.size()) dmax=26.0; else dmax=13.0; // big for current selection
        if (dd<dmax && f>0 && f<1) {t.n=-1; t.s=sh; t.sf=f; t.si=ept.pi[i]; t.type=htLine; return t;}
        int xx=ept.pt[i].x, yy=ept.pt[i].y;
        if (i==0) {brc.left=xx; brc.top=yy; brc.right=xx; brc.bottom=yy;}
        else {if (xx<brc.left) brc.left=xx; if (xx>brc.right) brc.right=xx; if (yy<brc.top) brc.top=yy; if (yy>brc.bottom) brc.bottom=yy;}
      }
      // now check to see if it's inside the polygon, but not on sh=-1 for the current shape
      POINT ps; ps.x=sx; ps.y=sy;
      if (!checkingsel && shape.brush.type!=ctNone && shape.p.size()>2 && inpoly(ept.pt,ptpos,ps,brc))
      { t.n=-1; t.s=sh; t.sf=0; t.si=-1; t.type=htShape; return t;
      }
    }
    //
    if (shape.limbs)
    { for (int i=1; i<body->nlimbs; i++)
      { TLimb &limb=body->limbs[i];
        int dx=b2x(limb.x)-b2x(limb.x0), dy=b2y(limb.y)-b2y(limb.y0);
        int dist=dx*dx+dy*dy;
        bool islimblong = (dist>12);
        if (islimblong && (limb.type==2 || limb.type==0)) // a line
        { double x0=0.01+(double)b2x(limb.x0), y0=0.01+(double)b2y(limb.y0), x1=b2x(limb.x), y1=b2y(limb.y);
          double x=sx, y=sy;
          double m=(y1-y0)/(x1-x0), c=y1-m*x1; // equation of line joining points
          double am=-1/m, ac=sy-am*x; // equation of perpendicular
          double xd=(ac-c)/(m-am), yd=m*xd+c; // coordinates of closest point on line
          double dx=xd-(double)x, dy=yd-(double)y;
          double dd=dx*dx+dy*dy;
          double f=(xd-x0)/(x1-x0);
          if (dd<13.0 && f>0 && f<1) {t.n=i; t.s=limbs; t.type=htLine; return t;}
        }
        else if (islimblong && limb.type==1) // an arc
        { double aoff=limb.aoff;
          if (limb.chan==4 && limb.band==0) aoff=limb.ang-limb.ang0; // for a fixed-frequency arc
          TAdjAngle a=AngleFromOff(x2b(sx)-limb.x0,y2b(sy)-limb.y0,limb.ang0,aoff,limb.ang-limb.ang0);
          double drad=a.screenrad-zoom*limb.length;
          if (drad<4 && drad>-4 && a.isin) {t.n=i; t.s=limbs; t.type=htLine;return t;}
        }
        else if (islimblong && limb.type==3) // a circle
        { TAdjAngle a=AngleFromOff(x2b(sx)-limb.x0,y2b(sy)-limb.y0,limb.ang0,limb.aoff,limb.ang-limb.ang0);
          double dx=limb.x-limb.x0,dy=limb.y-limb.y0; double len=sqrt(dx*dx+dy*dy);
          double drad=a.screenrad-zoom*len;
          if (drad<4 && drad>-4) {t.n=i; t.s=limbs; t.type=htLine;return t;}
        }
      }
    }
  }
  if (isrightclick && hit.n>0 && body->limbs[hit.n].type==2)
  { // for a right-click, we check if the user clicked on the spring itself.
    // do this last, so the spring doesn't obscure the line or the point.
    TLimb &sel=body->limbs[hit.n];
    double ddx=cos(sel.ang+0.5*pi)*SelPieLength/2, ddy=sin(sel.ang+0.5*pi)*SelPieLength/2; int dx=(int)ddx, dy=(int)ddy;
    POINT pt[4], p, s; s.x=sx; s.y=sy; int x,y;
    x=b2x(sel.x0+sel.lmin*cos(sel.ang)); y=b2y(sel.y0+sel.lmin*sin(sel.ang));
    pt[0].x=x+dx; pt[0].y=y+dy; pt[1].x=x-dx; pt[1].y=y-dy;
    x=b2x(sel.x0+sel.length*cos(sel.ang)); y=b2y(sel.y0+sel.length*sin(sel.ang));
    pt[2].x=x-dx; pt[2].y=y-dy; pt[3].x=x+dx; pt[3].y=y+dy;
    p.x=(pt[0].x+pt[1].x+pt[2].x+pt[3].x)/4; p.y=(pt[0].y+pt[1].y+pt[2].y+pt[3].y)/4;
    // the line (sx,sy)-(x,y) goes from the click to the center of the spring.
    // if this intersects any of the lines, then the click must have been outside
    bool isout = intersects(p,s, pt[0],pt[1]);
    isout |= intersects(p,s, pt[1],pt[2]);
    isout |= intersects(p,s, pt[2],pt[3]);
    isout |= intersects(p,s, pt[3],pt[0]);
    if (!isout) {t.n=hit.n; t.s=limbs; t.type=htPie; return t;}
  }
  return t;
}


void RecFlip(TBody *body, int n)
{ TLimb &limb = body->limbs[n];
  //TLimb &root = body->limbs[limb.root];
  //
  if (limb.chan==4 && limb.band==0) 
  { // a fixed limb. Ignores ang0,ascale, and uses just 'f'
    if (limb.root!=0 && limb.aisoff)
    { limb.f = 2*pi-limb.f;
    }
  }
  else
  { // Flip ang0. (we do this if the limb is relative to some root angle, but not if it's got a fixed base)
    if (limb.root!=0 && limb.aisoff)
    { limb.aoff = 2*pi-limb.aoff;
    }
    // Flip ascale. (we do this for everything, even springs and fixeds.)
    limb.ascale = -limb.ascale;
  }
  // now do the recursive bit
  for (int i=0; i<body->nlimbs; i++)
  { if (body->limbs[i].root==n) RecFlip(body,i);
  }
}

void TEditor::Flip()
{ if (sellimbs.size()==0) return;
  list<int> toflip; GetRootLimbsOfSellimbs(&toflip);
  MarkUndo(); 
  ismodified=true;
  for (list<int>::const_iterator i=toflip.begin(); i!=toflip.end(); i++)
  { RecFlip(body,*i); Recalc(*i);
  }
  Redraw(); SetCursor();
}

void TEditor::RecStretch(int n,double scale)
{ if (n!=0)
  { body->limbs[n].length *= scale;
    body->limbs[n].lmin *= scale;
  }
  for (int i=1; i<body->nlimbs; i++)
  { if (body->limbs[i].root==n) RecStretch(i,scale);
  }
}

void TEditor::Stretch(int dir)
{ if (dir==0) return;
  MarkUndo();
  bool tiny = (GetAsyncKeyState(VK_SHIFT)<0);
  double scale=0;  
  if (dir>0 && tiny) scale=1.01;
  else if (dir>0 && !tiny) scale=1.10;
  else if (dir<0 && tiny) scale=0.99;
  else if (dir<0 && !tiny) scale=0.9;
  // now we find which ones to stretch. If nothing was selected,
  // then we rec-stretch from the root 0. If stuff was, then we
  // find the smallest set of root ancestors and stretch them.
  list<int> tostretch; if (sellimbs.size()==0) tostretch.push_back(0);
  else GetRootLimbsOfSellimbs(&tostretch);
  for (list<int>::const_iterator i=tostretch.begin(); i!=tostretch.end(); i++)
  { RecStretch(*i,scale);
  }
  ismodified=true;
  if (tostretch.size()==1) Recalc(tostretch.front()); else Recalc(0);
  Redraw(); SetCursor();
}

  
void TEditor::DeleteLimbOrShape()
{ if (sellimbs.size()==0 && selshapes.size()==0) return;
  MarkUndo();
  int newsel=-1; // for if we want to select a new limb out of all this.
  if (sellimbs.size()==1 && selshapes.size()==0)
  { int sel=sellimbs.front();
    newsel=body->limbs[sel].root;
  }
  // first we'll delete all the selected shapes, then all the limbs.
  // That way, even if deleting a limb would cause a shape to be deleted,
  // the shape will already have been dealt with.
  selshapes.sort(); int sub=0;
  for (list<int>::const_iterator i=selshapes.begin(); i!=selshapes.end(); i++)
  { int li = *i;
    int si = li-sub;
    body->shapes.erase(body->shapes.begin()+si);
    sub++;
  }
  sellimbs.sort(); sub=0;
  for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
  { int li = *i;
    int si = li-sub;
    if (si!=0) {body->DeleteLimb(si); sub++;}
  }
  hit.s=-1; hit.n=-1; selshapes.clear(); sellimbs.clear();
  cori=-1; hit.type=htMiss; mode=mNothing;
  if (newsel!=-1)
  { hit.n=newsel; sellimbs.push_back(newsel);
    for (int i=0; i<(int)body->shapes.size(); i++) {if (body->shapes[i].limbs) hit.s=i;}
  }
  ismodified=true;
  Recalc(0);
  Redraw(); SetCursor();
  UpdateMenus();
  ShowHint("");
}

void TEditor::InsertLimb()
{ if (sellimbs.size()!=1) return;
  int node=sellimbs.front();
  MarkUndo();
  TLimb n; TLimb &limb=body->limbs[node];
  n.type=limb.type; 
  n.root=limb.root;
  n.aisoff=limb.aisoff; n.chan=limb.chan; n.band=limb.band; n.anchor=limb.anchor;
  n.aoff=limb.aoff; n.ascale=limb.ascale; n.f=limb.f;
  n.length=limb.length/2; n.lmin=limb.lmin/2;
  limb.length=limb.length/2; limb.lmin=limb.lmin/2;
  n.x0=limb.x0; n.y0=limb.y0;
  if (limb.color.type==ctNone) n.color.type=ctNone;
  else { n.color.type = ctRGB; n.color.rgb.r = 255; n.color.rgb.g = 255; n.color.rgb.b = 255; }
  n.color.dtype=n.color.type;
  n.thickness=1;
  int in=body->CreateLimb(n);
  body->limbs[node].root=in;
  body->MakeChildren();
  Recalc(in);
  //
  mode=mNothing; hit.n=in; hit.type=htMiss; sellimbs.clear(); selshapes.clear(); sellimbs.push_back(hit.n);
  ismodified=true;
  Redraw(); SetCursor();
  ShowHint("");
}


bool TEditor::Copy()
{ list<int> roots; GetRootLimbsOfSelection(&roots); if (roots.size()==0) return false;
  int len=body->WriteData(NULL,0,roots); if (len==0) return false;
  // now we have to figure out what extra stuff we'll need.
  // First, we'll make a 'bitmap' of the limbs and shapes to be saved.
  vector<bool> keep; keep.resize(body->nlimbs,false); bool changed=true;
  for (list<int>::const_iterator i=roots.begin(); i!=roots.end(); i++) keep[*i]=true;
  while (changed)
  { changed=false;
    for (int n=0; n<body->nlimbs; n++)
    { int r=body->limbs[n].root;
      if (keep[r] && !keep[n]) {keep[n]=true; changed=true;}
    }
  }
  vector<bool> keepshapes; keepshapes.resize(body->shapes.size(),false);
  for (int s=0; s<(int)body->shapes.size(); s++)
  { stk::TShape &shape = body->shapes[s]; bool ok=true;
    if (shape.limbs) continue;
    for (int i=0; i<(int)shape.p.size(); i++)
    { TJointRef pt = shape.p[i];
      if (!keep[pt.i]) ok=false;
    }
    if (ok) keepshapes[s]=true;
  }
  // now we can make the list of bitmaps to keep
  list<int> keepbitmaps;
  for (int i=0; i<body->nlimbs; i++)
  { if (keep[i] && body->limbs[i].type==3)
    { int si=circleshape_from_limb(i);
      if (si!=-1)
      { if (body->shapes[si].brush.type==ctBitmap) keepbitmaps.push_back(body->shapes[si].brush.bindex);
      }
    }
  }
  keepbitmaps.sort(); keepbitmaps.unique();
  // now we'll figure out how to lay the bitmaps out in memory
  unsigned int blen=0;
  for (list<int>::const_iterator i=keepbitmaps.begin(); i!=keepbitmaps.end(); i++)
  { blen+=MAX_PATH; // name
    blen+=sizeof(DWORD); // size
    blen+=body->bmps[*i].bufsize;
  }
  // figure out which styles we need
  list<string> keepstyles;
  for (int i=0; i<body->nlimbs; i++)
  { if (keep[i])
    { if (body->limbs[i].linestyle!="") keepstyles.push_back(body->limbs[i].linestyle);
      if (body->limbs[i].freqstyle!="") keepstyles.push_back(body->limbs[i].freqstyle);
    }
  }
  for (int s=0; s<(int)body->shapes.size(); s++)
  { if (keepshapes[s])
    { if (body->shapes[s].linestyle!="") keepstyles.push_back(body->shapes[s].linestyle);
      if (body->shapes[s].fillstyle!="") keepstyles.push_back(body->shapes[s].fillstyle);
    }
  }
  keepstyles.sort(); keepstyles.unique();
  // and again, layout in memory
  unsigned int slen=StylesToString(NULL,0,body->styles);
  //
  // that's it! got all the data we need!
  HGLOBAL hglob=GlobalAlloc(GMEM_DDESHARE|GMEM_MOVEABLE,3*sizeof(DWORD)+blen+slen+len);
  char *c=(char*)GlobalLock(hglob);
  *((DWORD*)c) = keepbitmaps.size(); c+=sizeof(DWORD);
  *((DWORD*)c) = slen; c+=sizeof(DWORD);
  *((DWORD*)c) = len; c+=sizeof(DWORD);
  for (list<int>::const_iterator i=keepbitmaps.begin(); i!=keepbitmaps.end(); i++)
  { TBmp &bmp = body->bmps[*i];
    strcpy(c,bmp.name.c_str()); c+=MAX_PATH;
    *((DWORD*)c) = bmp.bufsize; c+=sizeof(DWORD);
    memcpy(c,bmp.buf,bmp.bufsize); c+=bmp.bufsize;
  }
  StylesToString(c,slen,body->styles); c+=slen;
  body->WriteData(c,len,roots); // unused c+=len;
  GlobalUnlock(hglob);
  OpenClipboard(mhwnd);
  EmptyClipboard();
  SetClipboardData(StickClipboardFormat,hglob);
  CloseClipboard(); 
  return true;
}

void TEditor::Cut()
{ list<int> roots;
  GetRootLimbsOfSelection(&roots); // this is the same routine that Copy uses.
  if (roots.size()==0) return;
  Copy();
  MarkUndo();
  int root = roots.front(); root=body->limbs[root].root;
  vector<int> remap; remap.resize(body->nlimbs);
  for (list<int>::iterator i=roots.begin(); i!=roots.end(); i++)
  { body->DeleteBranch(*i,&remap);
    list<int>::iterator j=i; j++; for (; j!=roots.end(); j++)
    { int oldlimb = *j; LUASSERT(oldlimb!=-1);
      int newlimb = remap[oldlimb]; LUASSERT(newlimb!=-1);
      *j = newlimb;
    }
  }
  sellimbs.clear(); selshapes.clear();
  hit.n=root; sellimbs.push_back(root); hit.type=htMiss; mode=mNothing;
  ismodified=true;
  if (roots.size()==1) Recalc(root); else Recalc(0);
  Redraw(); SetCursor();
  ShowHint("");
}

void TEditor::Paste()
{ if (sellimbs.size()>1) return;
  if (!IsClipboardFormatAvailable(StickClipboardFormat)) return;
  char err[1000];
  OpenClipboard(mhwnd);
  HANDLE hglob=GetClipboardData(StickClipboardFormat);
  DWORD size=GlobalSize(hglob);
  char *clip=(char*)GlobalLock(hglob);
  char *buf=new char[size+1]; CopyMemory(buf,clip,size);
  GlobalUnlock(hglob);
  CloseClipboard();
  char *c=buf;
  int numbmps = *((DWORD*)c); c+=sizeof(DWORD);
  unsigned int stylelen = *((DWORD*)c); c+=sizeof(DWORD);
  unsigned int sticklen = *((DWORD*)c); c+=sizeof(DWORD);  sticklen; // unused
  // First we have to find if the new bitmaps will be any different from our existing ones
  // That'll determine whether we have to MarkUndo with bitmaps or can make do without
  list<TBmp> newbmps; 
  for (int i=0; i<numbmps; i++)
  { TBmp b; b.name=string(c); c+=MAX_PATH;
    b.bufsize = *((DWORD*)c); c+=sizeof(DWORD);
    b.buf = c; c+=b.bufsize;
    bool alreadyin=false;
    for (vector<TBmp>::const_iterator bi=body->bmps.begin(); bi!=body->bmps.end() && !alreadyin; bi++)
    { if (StringLower(bi->name)==StringLower(b.name)) alreadyin=true;
    }
    if (!alreadyin) newbmps.push_back(b);
  }
  //
  MarkUndo(newbmps.size()>0);
  // read in the bitmap data
  for (list<TBmp>::const_iterator i=newbmps.begin(); i!=newbmps.end(); i++)
  { TBmp b; b.name=i->name; b.bufsize=i->bufsize; b.buf=new char[b.bufsize]; memcpy(b.buf,i->buf,b.bufsize);
    PrepareBitmapData(&b,true);
    body->bmps.push_back(b);
  }
  // read in the style data
  list<TStyle> newstyles;
  StylesFromString(c,newstyles); c+=stylelen;
  // read in the stick data
  list<int> roots; bool res=body->ReadData(c,err,rdMerge,&roots);
  if (!res) {delete[] buf; MessageBox(mhwnd,err,"Unable to paste",MB_OK); return;}
  // delete the buffer
  delete[] buf;
  // 
  // now for the fixups...
  // first, for the styles
  for (list<TStyle>::const_iterator i=newstyles.begin(); i!=newstyles.end(); i++)
  { string name=i->name; bool alreadypresent=false;
    for (list<TStyle>::const_iterator j=body->styles.begin(); j!=body->styles.end() && !alreadypresent; j++)
    { if (name==j->name) alreadypresent=true;
    }
    if (!alreadypresent) body->styles.push_back(*i);
    else ApplyStyle(name,true);
  }
  // second, for the limb structure
  int pastepoint=hit.n;
  if (pastepoint<0) pastepoint=0;
  for (list<int>::const_iterator i=roots.begin(); i!=roots.end(); i++)
  { body->limbs[*i].root=pastepoint;
    body->limbs[*i].f=0.001*(double)(rand()%1000);
  }
  body->MakeChildren();
  // bitmaps
  MakeBindexes(body); MakeEindexes(body);
  //
  ismodified=true;
  Recalc(pastepoint);
  Redraw(); SetCursor();
  ShowHint("");
}


void TEditor::Backdrop(const string afn,bool suppressredraw)
{ string s=afn;
  if (s=="?")
  { char fn[MAX_PATH];
    OPENFILENAME ofn; ZeroMemory(&ofn,sizeof(ofn)); ofn.lStructSize=sizeof(ofn);
    ofn.hwndOwner=mhwnd;
    ofn.lpstrFilter="Bitmaps\0*.bmp\0\0";
    ofn.nFilterIndex=0;
    ofn.lpstrFile=fn; strcpy(fn,"");
    ofn.nMaxFile=MAX_PATH;
    ofn.lpstrFileTitle=NULL;
    string root = GetStickDir();
    ofn.lpstrInitialDir=root.c_str();
    ofn.Flags=OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST|OFN_HIDEREADONLY;
    ofn.lpstrDefExt="bmp";
    BOOL res=GetOpenFileName(&ofn);
    if (!res) return;
    s=ofn.lpstrFile;
  }
  if (hbackdrop!=0) {DeleteObject(hbackdrop); hbackdrop=0; fnbackdrop=""; ismodified=true;}
  if (afn=="") {if (!suppressredraw) {Redraw(); UpdateMenus();} return;}
  hbackdrop = (HBITMAP)LoadImage(NULL,s.c_str(),IMAGE_BITMAP,0,0,LR_LOADFROMFILE);
  if (hbackdrop!=0) fnbackdrop=s;
  ismodified=true; if (!suppressredraw) {Redraw(); UpdateMenus();}
}


void TEditor::Recalc(int n)
{ if (n==0) body->Recalc();
  else body->RecalcLimb(n);
  body->RecalcEffects(true);
}


void SaveBack(const string ffn,const string back)
{ string fn=StringLower(ExtractFileName(ffn));
  for (list<TUserBack>::iterator i=UserBacks.begin(); i!=UserBacks.end(); i++)
  { if (i->fn==fn) {i->back=back; return;}
  }
  if (UserBacks.size()>10) UserBacks.pop_back();
  TUserBack ub; ub.fn=fn; ub.back=back; UserBacks.push_front(ub);
}

string RecallBack(const string ffn)
{ string fn=StringLower(ExtractFileName(ffn));
  for (list<TUserBack>::iterator i=UserBacks.begin(); i!=UserBacks.end(); i++)
  { if (i->fn==fn)
    { TUserBack ub=*i;
      UserBacks.erase(i); UserBacks.push_front(ub); // give it higher priority
      return ub.back;
    }
  }
  return "";
}



bool SaveBody(TBody *body,const char *fn)
{ HANDLE hFile=CreateFile(fn,GENERIC_WRITE,0,NULL,CREATE_ALWAYS,FILE_ATTRIBUTE_NORMAL,NULL);
  if (hFile==INVALID_HANDLE_VALUE) return false;
  // We will first put in just a minimal almost dummy header, so that old
  // versions don't choke. But the real data will follow, zipped up
  string s = string("")+"nlimbs=0\r\nnshapes=1\r\nroot=0\r\n"
             "version=" STK_CORE_S "\r\n"
             "category="+body->category+"\r\n"
             "limb(0)= line invisible root(0) angabs(0,3)(0) lmin(0) length(0) freq(4,0) frac(0) col(0,0,0) thickness(0)\r\n";
             "shape(0)= limbs\r\n";
             "\0";
  DWORD writ; WriteFile(hFile,s.c_str(),s.length()+1,&writ,NULL);
  // Now comes the zipped up data
  HZIP hz = CreateZipHandle(hFile,NULL);
  // first the stick figure itself
  unsigned int len=body->WriteData(NULL,0,0); char *buf=new char[len];
  body->WriteData(buf,len,0); while (len>0 && buf[len-1]==0) len--;
  ZipAdd(hz,"mainstick.txt", buf,len);
  delete[] buf;
  // Now come the bitmaps
  for (vector<TBmp>::const_iterator i=body->bmps.begin(); i!=body->bmps.end(); i++)
  { bool isbmp = (i->buf[0]=='B' && i->buf[1]=='M');
    string ext; if (isbmp) ext=".bmp"; else ext=".jpg";
    ZipAdd(hz,(i->name+ext).c_str(), i->buf,i->bufsize);
    i->bufsize;
  }
  // Now come the styles
  len = StylesToString(NULL,0,body->styles);
  buf = new char[len]; StylesToString(buf,len,body->styles); while (len>0 && buf[len-1]==0) len--;
  ZipAdd(hz,"styles.txt", buf,len);
  delete[] buf;
  // wasn't that zip easy?!
  CloseZip(hz);
  CloseHandle(hFile);
  return true;
}




string TEditor::CleanBitmapName(const string s)
{ string broot=s;
  for (int i=0; i<(int)broot.size(); i++)
  { if (broot[i]==')') broot[i]=']';
    if (broot[i]=='(') broot[i]='[';
  }
  for (int i=1; ; i++)
  { string bname;
    if (i==1) bname=broot; else bname=broot+StringInt(i);
    bool okay=true;
    for (vector<TBmp>::const_iterator ci=body->bmps.begin(); ci!=body->bmps.end() && okay; ci++)
    { if (StringLower(ci->name)==StringLower(bname)) okay=false;
    }
    if (okay) return bname;
  }
}


string TEditor::AddBitmap(const char *fn)
{ bool isbmp=false, isjpeg=false;
  HBITMAP hbm=(HBITMAP)LoadImage(hInstance,fn,IMAGE_BITMAP,0,0,LR_LOADFROMFILE|LR_CREATEDIBSECTION);
  if (hbm!=0) {DeleteObject(hbm); isbmp=true;}
  if (!isbmp)
  { HANDLE hf = CreateFile(fn,GENERIC_READ,FILE_SHARE_READ,NULL,OPEN_EXISTING,0,NULL);
    if (hf!=INVALID_HANDLE_VALUE)
    { DWORD size = GetFileSize(hf,NULL);
      HGLOBAL hglob = GlobalAlloc(GMEM_MOVEABLE,size);
	    void *buf = GlobalLock(hglob);
      DWORD red; ReadFile(hf,buf,size,&red,NULL);
	    GlobalUnlock(hglob);
      CloseHandle(hf);
      hbm = MakeJpeg(hglob,true);
      GlobalFree(hglob);
      if (hbm!=0) {DeleteObject(hbm); isjpeg=true;}
    }
  }
  if (!isbmp && !isjpeg) return "";
  //

  TBmp bmp; 
  // pick a unique internal name for it
  bmp.name = CleanBitmapName(stk::ChangeFileExt(stk::ExtractFileName(fn),""));
  if (isjpeg) bmp.name+="-NT";
  // load the raw data
  HANDLE hf = CreateFile(fn,GENERIC_READ,FILE_SHARE_READ,NULL,OPEN_EXISTING,0,NULL);
  bmp.bufsize=GetFileSize(hf,NULL); bmp.buf = new char[bmp.bufsize];
  DWORD red; ReadFile(hf,bmp.buf,bmp.bufsize,&red,NULL);
  CloseHandle(hf);
  PrepareBitmapData(&bmp,true);
  // and add it into the list of things
  body->bmps.push_back(bmp);
  //
  return bmp.name;
}

void TEditor::DeleteBitmap(const string zname)
{ MarkUndo(true);
  for (vector<TBmp>::iterator ci=body->bmps.begin(); ci!=body->bmps.end(); ci++)
  { if (StringLower(ci->name)==StringLower(zname))
    { ci->release();
      body->bmps.erase(ci);
      // now remove any references to it
      for (vector<stk::TShape>::iterator i=body->shapes.begin(); i!=body->shapes.end(); i++)
      { stk::TShape &shape = *i;
        if (StringLower(shape.brush.bitmap)==StringLower(zname))
        { shape.brush.bitmap=""; shape.brush.bindex=-1; 
          if (shape.brush.type==ctBitmap) {shape.brush.type=ctNone; shape.brush.dtype=ctNone;}
        }
      }
      for (vector<TEffect>::iterator i=body->effects.begin(); i!=body->effects.end(); i++)
      { TEffect &effect = *i;
        for (vector<stk::TColor>::iterator j=effect.cols.begin(); j!=effect.cols.end(); j++)
        { if (StringLower(j->bitmap)==StringLower(zname))
          { j->bitmap=""; j->bindex=-1;
		if (j->type == ctBitmap) { j->type = ctRGB; j->rgb.r = 128; j->rgb.g = 128; j->rgb.b = 128; }
          }
        }
      }
      for (list<TStyle>::iterator i=body->styles.begin(); i!=body->styles.end(); i++)
      { TStyle &style = *i;
        if (StringLower(style.shape.brush.bitmap)==StringLower(zname))
        { style.shape.brush.bitmap=""; style.shape.brush.bindex=-1;
          if (style.shape.brush.type==ctBitmap) {style.shape.brush.type=ctNone; style.shape.brush.dtype=ctNone;}
        }
      }
      //
      MakeBindexes(body); body->RecalcEffects(true);
      ismodified=true;
      Redraw();
      return;
    }
  }
}

string TEditor::RenameBitmap(const string oldzn,const string newzn)
{ if (oldzn==newzn) return oldzn;
  MarkUndo(true);
  // ensure that the new name is unique
  string realnew = CleanBitmapName(newzn);
  // and rename it!
  for (vector<TBmp>::iterator ci=body->bmps.begin(); ci!=body->bmps.end(); ci++)
  { if (StringLower(ci->name)==StringLower(oldzn))
    { ci->name=realnew;
      PrepareBitmapData(&*ci,true);
      //
      for (vector<stk::TShape>::iterator i=body->shapes.begin(); i!=body->shapes.end(); i++)
      { stk::TShape &shape = *i;
        if (StringLower(shape.brush.bitmap)==StringLower(oldzn)) shape.brush.bitmap=realnew;
      }
      for (vector<TEffect>::iterator i=body->effects.begin(); i!=body->effects.end(); i++)
      { TEffect &effect = *i;
        for (vector<stk::TColor>::iterator j=effect.cols.begin(); j!=effect.cols.end(); j++)
        { if (StringLower(j->bitmap)==StringLower(oldzn)) j->bitmap=realnew;
        }
      }
      for (list<TStyle>::iterator i=body->styles.begin(); i!=body->styles.end(); i++)
      { TStyle &style = *i;
        if (StringLower(style.shape.brush.bitmap)==StringLower(oldzn)) style.shape.brush.bitmap=realnew;
      }
      //      
      MakeBindexes(body); 
      ismodified=true;
      return realnew;
    }
  }
  return oldzn;
}


bool TEditor::FileSave()
{ if (strcmp(curfile,"")==0) return FileSaveAs("");
  bool res = MaybeCheckUpright();
  if (!res) return false;
  string tit = "Save "+stk::ExtractFileName(curfile);
  DWORD attr = GetFileAttributes(curfile);
  if (attr!=0xFFFFFFFF && (attr&FILE_ATTRIBUTE_READONLY)!=0)
  { int id=MessageBox(mhwnd,"The file is read-only. Are you sure you wish to replace it?",tit.c_str(),MB_ICONQUESTION|MB_YESNOCANCEL);
    if (id!=IDYES) return false;
    SetFileAttributes(curfile,attr&~FILE_ATTRIBUTE_READONLY);
  }
  res = SaveBody(body,curfile);
  if (!res)
  { string msg = "There was an error saving the file:\r\n"+GetLastErrorString();
    MessageBox(mhwnd,msg.c_str(),tit.c_str(),MB_ICONERROR|MB_OK);
    return false;
  }
  SaveBack(curfile,fnbackdrop);
  ismodified=false; ShowTitle();
  string msg="Saved file "+stk::ChangeFileExt(stk::ExtractFileName(curfile),""); ShowHint(msg);
  return true;
}
  


bool TEditor::FileSaveAs(char *afn)
{ bool mres = MaybeCheckUpright();
  if (!mres) return false;
  char fn[MAX_PATH];
  OPENFILENAME ofn; ZeroMemory(&ofn,sizeof(ofn)); ofn.lStructSize=sizeof(ofn);
  ofn.hwndOwner=mhwnd;
  ofn.lpstrFilter="Sticks\0*.stk\0\0";
  ofn.nFilterIndex=0;
  string root = GetStickDir(); ofn.lpstrInitialDir=root.c_str();
  ofn.lpstrFile=fn;
  char *startswithroot=strstr(afn,root.c_str());
  if (startswithroot!=NULL) strcpy(fn,afn+root.length()+1); else strcpy(fn,afn);
  ofn.nMaxFile=MAX_PATH;
  ofn.lpstrFileTitle=NULL;
  ofn.Flags=OFN_OVERWRITEPROMPT|OFN_PATHMUSTEXIST|OFN_HIDEREADONLY;
  ofn.lpstrDefExt="stk";
  BOOL res=GetSaveFileName(&ofn);
  if (!res) return false;
  //
  string tit = "Save "+stk::ExtractFileName(fn);
  DWORD attr = GetFileAttributes(fn);
  if (attr!=0xFFFFFFFF && (attr&FILE_ATTRIBUTE_READONLY)!=0)
  { int id=MessageBox(mhwnd,"The file is read-only. Are you sure you wish to replace it?",tit.c_str(),MB_ICONQUESTION|MB_YESNOCANCEL);
    if (id!=IDYES) return false;
    SetFileAttributes(fn,attr&~FILE_ATTRIBUTE_READONLY);
  }
  res = SaveBody(body,fn);
  if (!res)
  { string msg = "There was an error saving the file:\r\n"+GetLastErrorString();
    MessageBox(mhwnd,msg.c_str(),tit.c_str(),MB_ICONERROR|MB_OK);
    return false;
  }
  //
  SaveBack(curfile,fnbackdrop);
  strcpy(curfile,fn); ismodified=false; ShowTitle();
  string msg="Saved file "+stk::ChangeFileExt(stk::ExtractFileName(fn),""); ShowHint(msg);
  return true;
}





LRESULT CALLBACK ProcessionSubclassProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ HWND hpar = GetParent(hwnd); if (hpar==0) return DefWindowProc(hwnd,msg,wParam,lParam);
  LONG_PTR oldproc = GetWindowLongPtr(hpar,GWLP_USERDATA); if (oldproc==0) return DefWindowProc(hwnd,msg,wParam,lParam);
  switch (msg)
  { case WM_RBUTTONDOWN:
    { if (exportpre.size()==0)
      { SetCapture(hwnd); SetCursor(LoadCursor(NULL,IDC_WAIT));
        DirScan(exportpre,GetStickDir()+"\\",true);
        SetCursor(LoadCursor(NULL,IDC_ARROW)); ReleaseCapture();
      }
      HMENU hmenu=CreatePopupMenu();
      for (int i=(int)exportpre.size()-1; i>=0; i--)
      { string s = exportpre[i].desc;
        PopulateMenu(hmenu,s.c_str(),1000+i,false);
      }
      POINT pt; pt.x=LOWORD(lParam); pt.y=HIWORD(lParam); ClientToScreen(hwnd,&pt);
      int cmd=TrackPopupMenu(hmenu,TPM_LEFTALIGN|TPM_TOPALIGN|TPM_RETURNCMD,pt.x,pt.y,0,hwnd,NULL);
      DestroyMenu(hmenu);
      if (cmd>=1000)
      { string stick = exportpre[cmd-1000].desc;
        char s[5000]; GetWindowText(hwnd,s,5000); s[4999]=0; int len=strlen(s);
        bool needcr=false;
        if (len==0) {} // doesn't need it
        else if (len==1) needcr=true;
        else if (s[len-1]!='\n' || s[len-2]!='\r') needcr=true;
        if (needcr) stick="\r\n"+stick;
        strcat(s,stick.c_str()); SetWindowText(hwnd,s);
        int id=GetWindowLongPtr(hwnd,GWLP_ID);
        SendMessage(hpar,WM_COMMAND,(WPARAM)(id|(EN_CHANGE<<16)),(LPARAM)hwnd);
      }
      return 0;
    }
  }
  return CallWindowProc((WNDPROC)oldproc,hwnd,msg,wParam,lParam);
}


string ListToString(const list<string> l)
{ string ss;
  for (list<string>::const_iterator i=l.begin(); i!=l.end(); i++)
  { if (ss.length()!=0) ss=ss+"\r\n";
    ss=ss+*i;
  }
  return ss;
}
list<string> StringToList(const string ss)
{ list<string> l;
  const char *c=ss.c_str();
  while (*c!=0)
  { const char *start=c; while (*c!='\r' && *c!='\n' && *c!=0) c++;
    string s = StringTrim(string(start,c-start));
    if (s.length()>0) l.push_back(s);
    while (*c=='\r' || *c=='\n') c++;
  }
  return l;
}


int WINAPI ExportDlgProc(HWND hdlg,UINT msg,WPARAM wParam,LPARAM lParam)
{ TEditor *ed = (TEditor*)GetWindowLong(hdlg,DWL_USER);
  switch (msg)
  { case WM_INITDIALOG:
    { ed = (TEditor*)lParam; SetWindowLong(hdlg,DWL_USER,(LONG)ed);
      RECT rc; GetWindowRect(hdlg,&rc); int w=rc.right-rc.left, h=rc.bottom-rc.top;
      RECT prc; GetWindowRect(GetParent(hdlg),&prc); int cx=(prc.left+prc.right)/2, cy=(prc.top+prc.bottom)/2;
      if (cx-w/2<0) cx=w/2; if (cy-h/2<0) cy=h/2;
      MoveWindow(hdlg,cx-w/2,cy-h/2,w,h,FALSE);
      // subclass the edit box
      HWND hed = GetDlgItem(hdlg,IDC_PROCESSIONLIST);
      LONG_PTR oldproc = GetWindowLongPtr(hed,GWLP_WNDPROC);
      SetWindowLongPtr(hdlg,GWLP_USERDATA,oldproc);
      SetWindowLongPtr(hed,GWLP_WNDPROC,(LONG_PTR)ProcessionSubclassProc);
      //
      SetDlgItemText(hdlg,IDC_TIME,StringInt(exporttime/1000).c_str());
      SetDlgItemText(hdlg,IDC_PROCTIME,StringNiceFloat(((double)exportproctime)/1000.0).c_str());
      SetDlgItemText(hdlg,IDC_WAVNAME,exportwav.c_str());
      CheckDlgButton(hdlg,IDC_SILENTDANCING, exporttune==0?BST_CHECKED:BST_UNCHECKED);
      CheckDlgButton(hdlg,IDC_SILENTTHRASHING, exporttune==1?BST_CHECKED:BST_UNCHECKED);
      CheckDlgButton(hdlg,IDC_DANCETOWAV, exporttune==2?BST_CHECKED:BST_UNCHECKED);
      CheckDlgButton(hdlg,IDC_TIMEFIXED,exporttimefixed?BST_CHECKED:BST_UNCHECKED);
      CheckDlgButton(hdlg,IDC_TIMEWAV,exporttimefixed?BST_UNCHECKED:BST_CHECKED);
      SetDlgItemText(hdlg,IDC_WIDTH,StringInt(exportw).c_str());
      SetDlgItemText(hdlg,IDC_HEIGHT,StringInt(exporth).c_str());
      SetDlgItemText(hdlg,IDC_FPS,StringInt(exportfps).c_str());
      if (exportcompress) CheckDlgButton(hdlg,IDC_SHOWCOMPRESSION,BST_CHECKED);
      SetDlgItemText(hdlg,IDC_PROCESSIONLIST,exportfigs.c_str());
      CheckDlgButton(hdlg,IDC_STICK_PROCESSION,exportproc?BST_CHECKED:BST_UNCHECKED);
      CheckDlgButton(hdlg,IDC_STICK_CURRENT,exportproc?BST_UNCHECKED:BST_CHECKED);
      return TRUE;
    }
    case WM_COMMAND:
    { int id=LOWORD(wParam), code=HIWORD(wParam); 
      if (id==IDCANCEL) EndDialog(hdlg,id);
      if (id==IDC_TIME && code==EN_CHANGE)
      { CheckDlgButton(hdlg,IDC_TIMEWAV,BST_UNCHECKED);
        CheckDlgButton(hdlg,IDC_TIMEFIXED,BST_CHECKED);
      }
      if ((id==IDC_PROCTIME||id==IDC_PROCESSIONLIST) && code==EN_CHANGE)
      { CheckDlgButton(hdlg,IDC_STICK_CURRENT,BST_UNCHECKED);
        CheckDlgButton(hdlg,IDC_STICK_PROCESSION,BST_CHECKED);
      }
      if (id==IDC_WAVNAME && code==EN_CHANGE)
      { CheckDlgButton(hdlg,IDC_DANCETOWAV,BST_CHECKED);
        CheckDlgButton(hdlg,IDC_SILENTDANCING,BST_UNCHECKED);
        CheckDlgButton(hdlg,IDC_SILENTTHRASHING,BST_UNCHECKED);
        CheckDlgButton(hdlg,IDC_TIMEWAV,BST_CHECKED);
        CheckDlgButton(hdlg,IDC_TIMEFIXED,BST_UNCHECKED);
      }
      if (id==IDC_SILENTDANCING || id==IDC_SILENTTHRASHING)
      { if (IsDlgButtonChecked(hdlg,IDC_SILENTDANCING)==BST_CHECKED || IsDlgButtonChecked(hdlg,IDC_SILENTTHRASHING)==BST_CHECKED)
        { CheckDlgButton(hdlg,IDC_TIMEWAV,BST_UNCHECKED);
          CheckDlgButton(hdlg,IDC_TIMEFIXED,BST_CHECKED);
        }
      }
      if (id==IDC_WAVBROWSE && code==BN_CLICKED)
      { char fn[MAX_PATH];
        OPENFILENAME ofn; ZeroMemory(&ofn,sizeof(ofn)); ofn.lStructSize=sizeof(ofn);
        ofn.hwndOwner=hdlg;
        ofn.lpstrFilter="WAV files\0*.wav\0\0";
        ofn.nFilterIndex=0;
        ofn.lpstrInitialDir=0;
        ofn.lpstrFile=fn; GetDlgItemText(hdlg,IDC_WAVNAME,fn,MAX_PATH);
        ofn.nMaxFile=MAX_PATH;
        ofn.lpstrFileTitle=NULL;
        ofn.Flags=OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST|OFN_HIDEREADONLY;
        ofn.lpstrDefExt="wav";
        BOOL res=GetOpenFileName(&ofn);
        if (res) SetDlgItemText(hdlg,IDC_WAVNAME,fn); // will also select this radio button
      }
      
      if (id==IDOK)
      { int nwidth,nheight,ntime,nfps,ntune,nproctime; string nwavname, nfigs;
        char c[5000]; GetDlgItemText(hdlg,IDC_WIDTH,c,MAX_PATH); int res=sscanf(c,"%i",&nwidth);
        if (res!=1 || nwidth<1 || nwidth>5000) {SetFocus(GetDlgItem(hdlg,IDC_WIDTH)); MessageBeep(0); return TRUE;}
        GetDlgItemText(hdlg,IDC_HEIGHT,c,MAX_PATH); res=sscanf(c,"%i",&nheight);
        if (res!=1 || nheight<1 || nheight>5000) {SetFocus(GetDlgItem(hdlg,IDC_HEIGHT)); MessageBeep(0); return TRUE;}
        GetDlgItemText(hdlg,IDC_WAVNAME,c,MAX_PATH); nwavname=c;
        if (GetFileAttributes(c)==0xFFFFFFFF)
        { if (IsDlgButtonChecked(hdlg,IDC_DANCETOWAV)==BST_CHECKED) {SetFocus(GetDlgItem(hdlg,IDC_WAVNAME)); MessageBeep(0); return TRUE;}
          nwavname=exportwav;
        }
        if (IsDlgButtonChecked(hdlg,IDC_TIMEWAV)==BST_CHECKED && IsDlgButtonChecked(hdlg,IDC_DANCETOWAV)!=BST_CHECKED)
        { SetFocus(GetDlgItem(hdlg,IDC_TIMEWAV)); MessageBeep(0); return TRUE;
        }
        GetDlgItemText(hdlg,IDC_TIME,c,MAX_PATH); res=sscanf(c,"%i",&ntime);
        if (res!=1 || ntime<1 || ntime>5000)
        { if (IsDlgButtonChecked(hdlg,IDC_TIMEFIXED)==BST_CHECKED) {SetFocus(GetDlgItem(hdlg,IDC_TIME)); MessageBeep(0); return TRUE;}
          ntime=exporttime;
        }
        GetDlgItemText(hdlg,IDC_PROCTIME,c,MAX_PATH); double d; res=sscanf(c,"%lf",&d); nproctime=(int)(d*1000.0);
        if (res!=1 || nproctime<1 || nproctime>50000)
        { if (IsDlgButtonChecked(hdlg,IDC_STICK_PROCESSION)==BST_CHECKED) {SetFocus(GetDlgItem(hdlg,IDC_PROCTIME)); MessageBeep(0); return TRUE;}
          nproctime=exportproctime;
        }
        GetDlgItemText(hdlg,IDC_FPS,c,MAX_PATH); res=sscanf(c,"%i",&nfps);
        if (res!=1 || nfps<1 || nfps>1000) {SetFocus(GetDlgItem(hdlg,IDC_FPS)); MessageBeep(0); return TRUE;}
        // now we'll check that the procession list is okay
        if (exportpre.size()==0)
        { SetCapture(hdlg); SetCursor(LoadCursor(NULL,IDC_WAIT));
          DirScan(exportpre,GetStickDir()+"\\",true);
          SetCursor(LoadCursor(NULL,IDC_ARROW)); ReleaseCapture();
        }
        GetDlgItemText(hdlg,IDC_PROCESSIONLIST,c,5000); c[4999]=0;
        list<string> tfigs=StringToList(c);
        bool sticksok=true;
        for (list<string>::const_iterator i=tfigs.begin(); i!=tfigs.end(); i++)
        { string s=reggarble(*i); // turn it into a regexp
          REGEXP re; regbegin(&re,s.c_str(),REG_ICASE);
          bool stickok=false;
          for (vector<TPre>::const_iterator i=exportpre.begin(); i!=exportpre.end(); i++)
          { bool res = regmatch(&re,i->desc.c_str(),NULL,0);
            if (res) {stickok=true; break;}
          }
          regend(&re);
          if (!stickok) sticksok=false;
        }
        nfigs=ListToString(tfigs);
        if (!sticksok)
        { if (IsDlgButtonChecked(hdlg,IDC_STICK_PROCESSION)) {SetFocus(GetDlgItem(hdlg,IDC_PROCESSIONLIST)); MessageBeep(0); return TRUE;}      
          nfigs=exportfigs;
        }
        ntune=-1;
        if (IsDlgButtonChecked(hdlg,IDC_SILENTDANCING)==BST_CHECKED) ntune=0;
        if (IsDlgButtonChecked(hdlg,IDC_SILENTTHRASHING)==BST_CHECKED) ntune=1;
        if (IsDlgButtonChecked(hdlg,IDC_DANCETOWAV)==BST_CHECKED) ntune=2;
        if (ntune==-1) {SetFocus(GetDlgItem(hdlg,IDC_SILENTDANCING)); MessageBeep(0); return TRUE;}
        //
        // at this point, we're all okay.
        exportw=nwidth;
        exporth=nheight;
        exporttime=ntime*1000;
        exportproctime=nproctime;
        exportfps=nfps;
        exportwav=nwavname;
        exportfigs=nfigs;
        exporttune=ntune;
        exportproc = (IsDlgButtonChecked(hdlg,IDC_STICK_PROCESSION)==BST_CHECKED);
        exporttimefixed = (IsDlgButtonChecked(hdlg,IDC_TIMEFIXED)==BST_CHECKED);
        exportcompress = (IsDlgButtonChecked(hdlg,IDC_SHOWCOMPRESSION)==BST_CHECKED);
        EndDialog(hdlg,IDOK);
      }
      return TRUE;
    } 
  }
  return FALSE;
}



class TComplex
{ public:
  TComplex () {}
  TComplex (double re): _re(re), _im(0.0) {}
  TComplex (double re, double im): _re(re), _im(im) {}
  double Re () const { return _re; }
  double Im () const { return _im; }
  void operator += (const TComplex& c) {_re += c._re; _im += c._im;}
  void operator -= (const TComplex& c) {_re -= c._re; _im -= c._im;}
  void operator *= (const TComplex& c) { double reT = c._re * _re - c._im * _im;_im = c._re * _im + c._im * _re; _re = reT;}
  TComplex operator- () { return TComplex (- _re, _im);}
  double Mod () const { return sqrt (_re * _re + _im * _im); }
  double _re;
  double _im;
};




// Given a wav-file, renders it into 'sampledat' format, one sample every 41ms.
// Return value is the number of samples needed for the wavfile.
// If the array given is not large enough, or if sampledat is NULL,
// then no samples are actually written.
int TEditor::fft(WavChunk *wav, TSpot *spots, int spotsize)
{
  // First, get basic information about the wav file.
  int bitsps = wav->fmt.wBitsPerSample;
  int nchan = wav->fmt.wChannels;
  unsigned long totbytes = wav->dat.size;
  unsigned long totsamples = (unsigned long)(((double)totbytes)*8.0/bitsps/nchan);
  unsigned long totms = (unsigned long)(((double)totsamples)*1000.0/wav->fmt.dwSamplesPerSec);
  int totspots = (int)(totms/41);
  if (totsamples<1024) return 0; // can't do anything with a file this small!
  if (totspots>spotsize) return totspots;
  if (spots==0) return totspots;

  //
  // Prepare some constants and tables and pre-computed tables
  const int npoints=1024;      // must be a power of two
  const double sqrtnpoints=32; // =sqrt(npoints)
  const int logpoints=10;      // binary log (ie. 2^10=1024)
  //const int npoints=256;
  //const double sqrtnpoints=16;
  //const int logpoints=8;
  //
  int bitrev[npoints];         // precomputed bit-reversal table
  TComplex W[logpoints+1][npoints];  // precomputed complex exponentials
  // Precomputed complex exponentials:
  for (int lg=1, twos=2; lg<=logpoints; lg++, twos*=2)
  { for (int i=0; i<npoints; i++ ) W[lg][i] = TComplex(cos(2.*pi*i/twos),-sin(2.*pi*i/twos));
  }
  // ... and bit reverse mapping
  for (int rev=0, i=0; i<npoints-1; i++)
  { bitrev[i] = rev;
    int mask = npoints/2;
    while (rev >= mask) {rev-=mask; mask >>= 1;}
    rev += mask;
  }
  bitrev[npoints-1] = npoints-1;


  // Now go through the file building FFT data!
  // On the first pass, we will store raw values in spots.l[], spots.r[]
  // Also, we will use two arrays to store our FFT analysis of each spot:
  double tape[2][npoints];     // the raw waveform (ie. time-domain) normalised into doubles
  TComplex X[2][npoints];      // the frequency-domain
  //
  for (int spot=0; spot<totspots && !ExportCancelled; spot++)
  { string msg = "Finding the groove: "+StringInt(spot*100/totspots)+"%";
    ShowHint(msg); PumpMessages();
    //      
    unsigned long mspos = spot*41;
    unsigned long samplepos = mspos*wav->fmt.dwSamplesPerSec/1000;
    unsigned long startsample=0; if (samplepos>=npoints/2) startsample=samplepos-npoints/2;
    if (startsample+npoints>totsamples) startsample=totsamples-npoints;
    unsigned long startbyte = startsample*nchan*bitsps/8;
    //
    // copy the raw data into our tape, normalising it into doubles
    unsigned char *bdat = (unsigned char*)wav->dat.data+startbyte;
    signed short *wdat = (signed short*)bdat;
    for (int i=0; i<npoints; i++)
    { if (bitsps==8)
      { tape[0][i] = (bdat[nchan*i+0]-128)*64;
        if (nchan==1) tape[1][i]=tape[0][i];
        else tape[1][i] = (bdat[nchan*i+1]-128)*64;
      }
      else
      { tape[0][i] = wdat[nchan*i+0];
        if (nchan==1) tape[1][i]=tape[0][i];
        else tape[1][i] = wdat[nchan*i+1];
      }
    }
    // initialize the FFT buffer
    for (int chan=0; chan<2; chan++)
    { for (int i=0; i<npoints; i++) X[chan][bitrev[i]]=TComplex(tape[chan][i]);
    }
    // do the transform
    for (int chan=0; chan<2; chan++)
    { int step = 1;
      for (int level=1; level<=logpoints; level++)
      { int increm = step*2;
        for (int j=0; j<step; j++)
        { TComplex U = W[level][j];   // U=exp(-2PIj / 2^level)
          for (int i=j; i<npoints; i+=increm)
          { TComplex T = U;
            T *= X [chan][i+step];
            X [chan][i+step] = X[chan][i];
            X [chan][i+step] -= T;
            X [chan][i] += T;
          }
        }
        step *= 2;
      }
    }
    // store in buckets, spots.l[] and spots.r[]
    // 0:  450 -  772
    // 1:  794 - 1117
    // 2: 1134 - 1461
    // 3: 1480 - 1827
    // 4: 1827 - 2171
    // 5: 2171 - 2515
    double fftfreq = ((double)wav->fmt.dwSamplesPerSec)/2;     // the maximum frequency of the fft
    double maxfreq = fftfreq; if (maxfreq>2515) maxfreq=2515;  // the max frequency we'll inspect
    double minfreq = 450;
    int boff = (int)(((double)npoints)*minfreq/fftfreq); // index of minfreq in X[] array
    int bmax = (int)(((double)npoints)*maxfreq/fftfreq); // index of maxfreq
    int bwidth = (bmax-boff)/6;                          // number of X[]s for each bucket
    for (int p=0; p<6; p++)
    { spots[spot].l[p]=0;
      spots[spot].r[p]=0;
      for (int i=0; i<bwidth; i++)
      { int b = boff+p*bwidth+i;
        spots[spot].l[p] += X[0][b].Mod()/sqrtnpoints/400;
        spots[spot].r[p] += X[1][b].Mod()/sqrtnpoints/400;
      }
      if (spots[spot].l[p]!=0) spots[spot].l[p] = sqrt(spots[spot].l[p])*100;
      if (spots[spot].r[p]!=0) spots[spot].r[p] = sqrt(spots[spot].r[p])*100;
    }
  }
  if (ExportCancelled) return 0;

  //
  // Now we have spots.l[] and spots.r[] as the raw bucketed output of FFT.
  // We will do another pass now to establish maximums and minimums,
  // and to rescale l[] and r[] as fractions within that range
  double min[6], max[6]; for (int i=0; i<6; i++) {min[i]=500; max[i]=1800+i*100;}
  // we will in fact start fifteen seconds into the music, work backwards to
  // the start, so setting up decent min/maxes. Then we'll work forwards through the piece.
  int startspot=15000/41; if (startspot>=totspots) startspot=totspots-1; startspot=-startspot;
  for (int ispot=startspot; ispot<totspots; ispot++)
  { int spot=ispot; if (spot<0) spot=-ispot;
    double *l=&spots[spot].l[0], *r=&spots[spot].r[0];
    // fade the max and min
    bool adjust = ((spot%2)==0);
    for (int p=0; adjust && p<6; p++)
    { max[p]*=0.999;
      if (min[p]<22) min[p]=22; min[p]=min[p]+1*1.002;
    }
    // push out the max and min if we've exceeded them
    for (int p=0; p<6; p++)
    { if (l[p]>max[p]) max[p]=l[p];
      if (r[p]>max[p]) max[p]=r[p];
      if (adjust && (l[p]<min[p] || r[p]<min[p])) min[p]*=0.9;
    }
    // finally, on the forward pass, write the data
    if (ispot>=0)
    { for (int i=0; i<6; i++)
      { spots[spot].min[i] = min[i];
        spots[spot].max[i] = max[i];
        spots[spot].l[i] = (l[i]-min[i])/(max[i]-min[i]);
        spots[spot].r[i] = (r[i]-min[i])/(max[i]-min[i]);
      }
    }
  }

  //
  return totspots;
}




void TEditor::FileExport()
{
  #ifdef __BORLANDC__
  _control87(MCW_EM, MCW_EM); // reprogram the FPU so it doesn't raise exceptions
  #endif
  char fn[MAX_PATH];
  OPENFILENAME ofn; ZeroMemory(&ofn,sizeof(ofn)); ofn.lStructSize=sizeof(ofn);
  ofn.hwndOwner=mhwnd;
  ofn.lpstrFilter="AVI files\0*.avi\0\0";
  ofn.nFilterIndex=0;
  string root = ExtractFilePath(exportfn); ofn.lpstrInitialDir=root.c_str();
  ofn.lpstrFile=fn; if (strcmp(curfile,"")==0) *fn=0; else strcpy(fn,stk::ChangeFileExt(stk::ExtractFileName(curfile),"").c_str());
  ofn.nMaxFile=MAX_PATH;
  ofn.lpstrFileTitle=NULL;
  ofn.Flags=OFN_OVERWRITEPROMPT|OFN_PATHMUSTEXIST|OFN_HIDEREADONLY;
  ofn.lpstrDefExt="stk";
  BOOL res=GetSaveFileName(&ofn);
  if (!res) return;
  exportfn = string(fn);
  //
  int res2 = DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_EXPORT),mhwnd,ExportDlgProc, (LPARAM)this);
  if (res2!=IDOK) return;
  RegSaveUser();
  //
  ExportCancelled=false; ExportTerminated=false;
  mode = mExporting;
  SetCursor();
  HMENU hmenu = GetMenu(mhwnd);
  HMENU hfile = GetSubMenu(hmenu,0);
  MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi); mi.fMask=MIIM_STRING;
  mi.dwTypeData="Cancel &Export"; mi.cch=strlen(mi.dwTypeData);
  SetMenuItemInfo(hfile,ID_FILE_EXPORTAVI,FALSE,&mi);
  //
  HDC sdc=GetDC(0); HDC hdc=CreateCompatibleDC(sdc); ReleaseDC(0,sdc);
  BITMAPINFOHEADER bih; ZeroMemory(&bih,sizeof(bih));
  bih.biSize=sizeof(bih);
  bih.biWidth=exportw;
  bih.biHeight=exporth;
  bih.biPlanes=1;
  bih.biBitCount=24;
  bih.biCompression=BI_RGB;
  bih.biSizeImage = ((bih.biWidth*bih.biBitCount/8+3)&0xFFFFFFFC)*bih.biHeight;
  bih.biXPelsPerMeter=10000;
  bih.biYPelsPerMeter=10000;
  bih.biClrUsed=0;
  bih.biClrImportant=0;
  void *bits; HBITMAP hbm=CreateDIBSection(hdc,(BITMAPINFO*)&bih,DIB_RGB_COLORS,&bits,NULL,NULL);
  HBITMAP hold=(HBITMAP)SelectObject(hdc,hbm);
  int exportinterval=1; if (exportfps!=0) exportinterval = 1000/exportfps;

  // load up the stuff we'll need: temporary copy of the current body, and the wav file.
  list<string> proclist; list<TBody*> bodies; vector<TPre> sticks;
  if (exportproc)
  { proclist = StringToList(exportfigs);
    DirScan(sticks,GetStickDir()+"\\");
  }
  else
  { char tdir[MAX_PATH]; GetTempPath(MAX_PATH,tdir);
    char tfn[MAX_PATH]; GetTempFileName(tdir,"stk",0,tfn);
    SaveBody(body,tfn); char err[1000];
    TBody *b = new TBody(); LoadBody(&b,tfn,err,lbForUse);
    DeleteFile(tfn);
    bodies.push_back(b);
  }
  char *wavbuf=0; WavChunk *wav=0; WAVEFORMATEX wfx; TSpot *spots=0; int totspots=0;
  int curtune=exporttune;
  unsigned long curendtime = exporttime;
  unsigned long totsamples=0;
  if (exporttune==2)
  { HANDLE hf = CreateFile(exportwav.c_str(),GENERIC_READ,FILE_SHARE_READ,NULL,OPEN_EXISTING,0,NULL);
    if (hf==NULL) {curtune=0; string err="Unable to open file "+ExtractFileName(exportwav); MessageBox(mhwnd,err.c_str(),"Export",MB_OK|MB_ICONERROR);}
    else
    { string msg = "Loading music..."; ShowHint(msg); PumpMessages();
      DWORD size=GetFileSize(hf,NULL);
      wavbuf = new char[size];
      DWORD red; ReadFile(hf,wavbuf,size,&red,NULL);
      CloseHandle(hf);
      wav = (WavChunk*)wavbuf;
      ZeroMemory(&wfx,sizeof(wfx));
      wfx.cbSize=0;
      wfx.nAvgBytesPerSec=wav->fmt.dwAvgBytesPerSec;
      wfx.nBlockAlign=wav->fmt.wBlockAlign;
      wfx.nChannels=wav->fmt.wChannels;
      wfx.nSamplesPerSec=wav->fmt.dwSamplesPerSec;
      wfx.wBitsPerSample=wav->fmt.wBitsPerSample;
      wfx.wFormatTag=wav->fmt.wFormatTag;
      int bytespersample = wfx.nChannels*wfx.wBitsPerSample/8;
      totsamples = wav->dat.size/bytespersample;
      unsigned long bytespersec = wav->fmt.dwSamplesPerSec * wav->fmt.wBitsPerSample/8 * wav->fmt.wChannels;
      if (!exporttimefixed) curendtime = (unsigned long)(((double)wav->dat.size)*1000.0 / ((double)bytespersec));
    }
  }
  unsigned long silentindex=0, silentmax=0, silentoffset=0; // if we're doing silent dancing, this is it
  if (curtune==0)
  { silentindex=rand()%3;
    silentmax=samplesize[silentindex];
    silentoffset=rand()%silentmax;
  }
  
  HAVI avi = CreateAvi(exportfn.c_str(),exportinterval,curtune==2?&wfx:0);
  if (exportcompress)
  { AVICOMPRESSOPTIONS opts; ZeroMemory(&opts,sizeof(opts));
    opts.fccHandler = exportopt;
    if (exportopt!=0) opts.dwFlags = AVICOMPRESSF_VALID;
    HRESULT hr = SetAviVideoCompression(avi,hbm,&opts,true,mhwnd);
    if (hr==AVIERR_OK) exportopt = opts.fccHandler;
    else ExportCancelled=true;
  }
  if (curtune==2 && !ExportCancelled)
  { totspots = fft(wav,NULL,0);
    spots = new TSpot[totspots];
    fft(wav,spots,totspots);
  }
  //
  unsigned long nframe=0, nsample=0;
  unsigned long totbytes=0;
  int procoff=exportw-exporth; if (procoff<0) procoff=0; int procgap=exporth/3;
  //
  for (unsigned long time=0; time<=curendtime && !ExportCancelled; time+=exportinterval)
  { // Let's add more bodies into the procession, if needed.
    // procoff is how many pixels the first body starts from the left of the bitmap.
    if (exportproc)
    { while (procoff<=-exporth-procgap)
      { TBody *b=bodies.front(); bodies.pop_front(); delete b;
        procoff+=exporth+procgap;
      }
      while (procoff + (int)bodies.size()*(exporth+procgap) < exportw)
      { for (list<string>::const_iterator ip=proclist.begin(); ip!=proclist.end(); ip++)
        { int i = ChooseRandomBody(reggarble(*ip),sticks,"");
          string fn = sticks[i].path;
          TBody *b = new TBody(); char err[1000];
          LoadBody(&b,fn.c_str(),err,lbForUse);
          bodies.push_back(b);
        }
      }
    }
    // first, if necessary, we add up to "<time" of music to the stream
    // (i.e. there'll be nothing on the first time round this loop
    if (curtune==2)
    { unsigned long tosample = (unsigned long)(((double)wfx.nSamplesPerSec)*((double)time)/1000.0);
      if (tosample>totsamples) tosample=totsamples; // in case of any rounding errors!
      unsigned long numbytes = (tosample-nsample)*wfx.wBitsPerSample/8*wfx.nChannels;
      unsigned long curbyte = nsample*wfx.wBitsPerSample/8*wfx.nChannels;
      if (numbytes>0) AddAviAudio(avi, wav->dat.data+curbyte, numbytes);
      totbytes += numbytes;
      nsample=tosample;
    }
    // second, we calculate the frequencies
    double freq[3][6];
    if (curtune==0)
    { unsigned int off = ((silentoffset+time)/41)%silentmax;
      unsigned char *sd = sampledat[silentindex]+off*12;
      for (int chan=0; chan<2; chan++)
      { for (int band=0; band<6; band++)
        { unsigned char s = sd[chan*6+band];
          double d = ((double)s)/255.0;
          freq[chan][band]=d;
        }
      }
      for (int band=0; band<6; band++) {freq[2][0]=freq[0][3]; freq[2][1]=freq[1][4];}
    }
    else if (curtune==1)
    { for (int chan=0; chan<3; chan++)
      { for (int band=0; band<6; band++)
        { freq[chan][band] = ((double)(rand()%1000))/1000.0;
        }
      }
    }
    else if (curtune==2)
    { int spot = time/41;
      for (int band=0; band<6; band++)
      { freq[0][band] = spots[spot].l[band];
        freq[1][band] = spots[spot].r[band];
      }
      freq[2][0]=spots[spot].l[3]; freq[2][1]=spots[spot].r[4];
    }
    // now assign that into the bodies
    double cmul=((double)exportinterval)/10.0; // 10ms is notional standard interval
    for (list<TBody*>::const_iterator ib=bodies.begin(); ib!=bodies.end(); ib++)
    { TBody *b = *ib;
      b->AssignFreq(freq,cmul);
      b->Recalc(); b->RecalcEffects(true);
    }
    // and draw them!
    if (exportproc)
    { RECT rc; rc.left=0; rc.top=0; rc.right=exportw; rc.bottom=exporth;
      FillRect(hdc,&rc,(HBRUSH)GetStockObject(BLACK_BRUSH));
      int x=procoff; list<TBody*>::const_iterator ib=bodies.begin();
      for (; x<exportw; ib++)
      { TBody *b = *ib;
        rc.left=x; rc.top=0; rc.right=x+exporth; rc.bottom=exporth;
        SimpleDraw(hdc,rc,b);
        x+=exporth+procgap;
      }
      procoff -= exportw*exportinterval/exportproctime;
    }
    else
    { TBody *b = bodies.front();
      RECT rc; rc.left=0; rc.top=0; rc.right=exportw; rc.bottom=exporth;
      SimpleDraw(hdc,rc,b);
    }
    if (!ExportCancelled)
    { AddAviFrame(avi,hbm); nframe++;
      string msg = "Exporting: "+StringFrac(((double)time)*100.0/((double)curendtime)); ShowHint(msg);
      PumpMessages();
    }
  }
  CloseAvi(avi);
  for (list<TBody*>::const_iterator i=bodies.begin(); i!=bodies.end(); i++)
  { TBody *b = *i;
    delete b;
  }
  if (wavbuf!=0) delete[] wavbuf;
  if (spots!=0) delete[] spots;
  SelectObject(hdc,hold);
  DeleteObject(hbm);
  DeleteObject(hdc);
  mi.dwTypeData="&Export AVI..."; mi.cch=strlen(mi.dwTypeData);
  SetMenuItemInfo(hfile,ID_FILE_EXPORTAVI,FALSE,&mi);
  mode=mNothing;
  string msg;
  if (ExportCancelled) msg="Cancelled export.";
  else msg="Exported file "+ChangeFileExt(ExtractFileName(exportfn),"");
  ShowHint(msg);
  SetCursor();
  if (ExportTerminated) PostMessage(mhwnd,WM_CLOSE,0,0);
  return;
}


void TEditor::HoverMonitor(int x,int y)
{ // First see if a movement has caused us to display something new
  ThisHover = PointAtCoords(x,y,true);
  if (hit.n!=-1 && ThisHover.n==body->limbs[hit.n].root && ThisHover.type==htSpot)
  { ThisHover.n=hit.n;
    ThisHover.type=htRoot;
  }
  if (ThisHover.n!=LastHover.n || ThisHover.type!=LastHover.type || ThisHover.s!=LastHover.s)
  { ShowHint("");
    LastHover=ThisHover;
    return;
  }
  // Otherwise, see if the end of timer means that something else should be displayed
  if (IsHoverTimer)
  { if (GetTickCount()>EndHoverTimer) 
    { ShowHint(""); IsHoverTimer=false;
    }
  }
}


void TEditor::SetCursor()
{ int c=0;
  const int cpoint=7, cmove=1, croot=2, cangle=3, ccre=4, ccreate=5, czoom=6, chourglass=-1;
  //
  if (mode==mNodePressing && tool==tCreate) c=ccreate;
  else if (mode==mNodePressing && tool==tEdit) c=cmove;
  else if (mode==mShapeLinePressing) c=ccreate;
  else if (mode==mShapeSpotPressing) c=cmove;
  else if (mode==mCreateDragging) c=ccreate;
  else if (mode==mNodeMoving) c=cmove;
  else if (mode==mAoffDragging) c=cangle;
  else if (mode==mAscaleDragging) c=cangle;
  else if (mode==mLminDragging) c=cangle;
  else if (mode==mLmaxDragging) c=cangle;
  else if (mode==mNothingTesting) c=0;
  else if (mode==mZoomDragging) c=czoom;
  else if (mode==mExporting) c=chourglass;
  //
  else if (tool==tCreate) 
  { if (ThisHover.s!=-1 && ThisHover.n==-1)
    { if (hit.s==ThisHover.s && hit.n==-1 && ThisHover.type==htSpot) c=cmove;
      else if (hit.s==ThisHover.s && hit.n==-1 && ThisHover.type==htAroot) c=cmove;
      else if (hit.s==ThisHover.s && hit.n==-1 && ThisHover.type==htLine) c=ccreate;
      else c=croot;
    }
    else if (ThisHover.n==-1/*background*/) c=ccre;
    else if (ThisHover.type==htSpot) c=ccreate;
    else if (ThisHover.type==htAroot) c=ccre;
    else if (ThisHover.type==htAmin) c=cangle;
    else if (ThisHover.type==htAmax) c=cangle;
    else if (ThisHover.type==htLine) c=cmove;
    else if (ThisHover.type==htRoot) c=ccreate;
    else if (ThisHover.type==htPie) c=ccre;
  }
  else if (tool==tEdit) 
  { if (ThisHover.s!=-1 && ThisHover.n==-1)
    { if (hit.s==ThisHover.s && hit.n==-1 && ThisHover.type==htSpot) c=cmove;
      else if (hit.s==ThisHover.s && hit.n==-1 && ThisHover.type==htAroot) c=cmove;
      else c=croot;
    }
    else if (ThisHover.n==-1/*background*/) c=cpoint;
    else if (ThisHover.type==htSpot) c=cmove;
    else if (ThisHover.type==htAmin) c=cangle;
    else if (ThisHover.type==htAmax) c=cangle;
    else if (ThisHover.type==htLine) c=cmove;
    else if (ThisHover.type==htRoot && body->limbs[ThisHover.n].root!=0) c=croot;
    else if (ThisHover.type==htRoot && body->limbs[ThisHover.n].root==0) c=cmove;
    else if (ThisHover.type==htPie) c=cpoint;
  }
  else if (tool==tZoom)
  { c=czoom;
  }
  
  HCURSOR hc; if (c==0) hc=LoadCursor(NULL,IDC_ARROW);
  else if (c==chourglass) hc=LoadCursor(NULL,IDC_WAIT);
  else hc=hCursor[c];
  ::SetCursor(hc);
}



char *SelectHint(int n,char *a,char *b)
{ if ((rand()%n)==0) return b; else return a;
}

void TEditor::ShowHint(string msg)
{ // Maybe we're being asked to override...
  if (msg!="")
  { SendMessage(hstatwnd,SB_SETTEXT,255|SBT_NOBORDERS,(LPARAM)msg.c_str());
    IsHoverTimer=true;
    EndHoverTimer=GetTickCount()+2000;
    return;
  }
  // or merely to display whatever's appropiate at this new stage
  const char *c="Right-click on background to change editing mode";
  //
  // We'll first see if there's a specific mode for the current. If not, we'll see
  // what's hovering on
  if (mode==mNodePressing && tool==tCreate) c="Click and drag to create a new line out of a joint";
  else if (mode==mNodePressing && tool==tEdit) c="Click and drag to move this joint";
  else if (mode==mCreateDragging) c="Left-click and drag to create a new line";
  else if (mode==mNodeMoving) c=SelectHint(3,"Click and drag to move this joint","Click and drag. (SHIFT to stop snapping)");
  else if (mode==mAoffDragging) c=SelectHint(3,"Drag minimum range of motion","Drag minimum. (SHIFT to stop snapping)");
  else if (mode==mAscaleDragging) c=SelectHint(3,"Drag maximum range of motion","Drag maximum. (SHIFT to stop snapping)");
  else if (mode==mLminDragging) c="Drag maximum range of motion";
  else if (mode==mLmaxDragging) c="Drag minimum range of motion";
  else if (mode==mNothingTesting) c="Click to stop animation";
  else if (mode==mZoomDragging) c="Zoom in...";
  //
  else if (tool==tCreate)
  { if (ThisHover.s!=-1 && ThisHover.n==-1) // a shape 
    { if (ThisHover.type==htLine) c="CREATE mode. Click and drag to create a new corner here";
      else if (ThisHover.type==htSpot) c="Click and drag to move this corner";
      else if (ThisHover.type==htShape) c="CREATE mode. Click and drag on a line to create a new corner on it";
    }
    else if (ThisHover.n==-1/*background*/) c=SelectHint(3,"CREATE mode. Click and drag to create a new line out of a joint","CREATE mode. (Right-click to go back to EDIT mode)");
    else if (ThisHover.type==htSpot) c="Click and drag to create a new line from this joint";
    else if (ThisHover.type==htAmin) c="Drag minimum range of motion";
    else if (ThisHover.type==htAmax) c="Drag maximum range of motion";
    else if (ThisHover.type==htLine) c="Click to select this line";
    else if (ThisHover.type==htRoot) c="Click and drag to create a new line from this joint";
    else if (ThisHover.type==htPie) 
    { char d[200]; int ic=body->limbs[ThisHover.n].chan, ib=body->limbs[ThisHover.n].band;
      if (ic==2) wsprintf(d,"[freq=diff%i] ",ib+1);
      else if (ic==3) wsprintf(d,"[freq=karaoke%i] ",ib+1);
      else if (ic==0) wsprintf(d,"[freq=left%i] ",ib+1);
      else if (ic==1) wsprintf(d,"[freq=right%i] ",ib+1);
      else if (ic==4) wsprintf(d,"[freq=fixed] ");
      else *d=0;
      msg = string(d)+"Right-click to respond to different frequency";
      c = msg.c_str();
    }
  }
  else if (tool==tEdit)
  { if (ThisHover.s!=-1 && ThisHover.n==-1) // a shape 
    { if (ThisHover.type==htLine) c="Click to select the outline of this shape";
      else if (ThisHover.type==htSpot) c="Click and drag to move this corner";
      else if (ThisHover.type==htShape) c="EDIT mode. Click to select this shape";
    }
    else if (ThisHover.n==-1 /*background*/) c=SelectHint(4,"EDIT mode. Click on a joint to move it","EDIT mode. Right-click on background to change mode");
    else if (ThisHover.type==htSpot)
    { if (body->limbs[ThisHover.n].anchor==0) c=SelectHint(3,"Click and drag to move this joint","Right-click to anchor this joint");
      else c="Right-click to anchor this joint";
    }
    else if (ThisHover.type==htAmin) c="Drag minimum range of motion";
    else if (ThisHover.type==htAmax) c="Drag maximum range of motion";
    else if (ThisHover.type==htLine) c="Click to select this line";
    else if (ThisHover.type==htRoot && body->limbs[ThisHover.n].root!=0)
    { if (body->limbs[ThisHover.n].aisoff)
      { c="Relative angle. Left-click to make it an absolute angle";
      }
      else
      { c="Absolute angle. Left-click to make it a relative angle";
      }
    }
    else if (ThisHover.type==htPie)
    { char d[200]; int ic=body->limbs[ThisHover.n].chan, ib=body->limbs[ThisHover.n].band;
      if (ic==4) wsprintf(d,"[freq=fixed] ");
      else if (ic==3) wsprintf(d,"[freq=karaoke%i] ",ib+1);
      else if (ic==2) wsprintf(d,"[freq=diff%i] ",ib+1);
      else if (ic==1) wsprintf(d,"[freq=right%i] ",ib+1);
      else if (ic==0) wsprintf(d,"[freq=left%i] ",ib+1); 
      else *d=0;
      msg = string(d)+"Right-click to respond to different frequency";
      c = msg.c_str();
    }
  }
  else if (tool==tZoom)
  { c="ZOOM mode. Drag a rectangle to zoom in; click to zoom out";
  }

  SendMessage(hstatwnd,SB_SETTEXT,255|SBT_NOBORDERS,(LPARAM)c);
}


void TEditor::ShowTitle()
{ string s = stk::ChangeFileExt(stk::ExtractFileName(curfile),"")+" - Sticky";
  if (strcmp(curfile,"")==0) s="Sticky";
  SetWindowText(mhwnd,s.c_str());
}

void TEditor::FileOpen(char *afn)
{ if (!FileClose()) return;
  char fn[MAX_PATH];
  if (afn!=NULL && strcmp(afn,"")!=0) strcpy(fn,afn);
  else
  { OPENFILENAME ofn; ZeroMemory(&ofn,sizeof(ofn)); ofn.lStructSize=sizeof(ofn);
    ofn.hwndOwner=mhwnd;
    ofn.lpstrFilter="Sticks\0*.stk\0\0";
    ofn.nFilterIndex=0;
    ofn.lpstrFile=fn; strcpy(fn,"");
    ofn.nMaxFile=MAX_PATH;
    ofn.lpstrFileTitle=NULL;
    string root = GetStickDir();
    ofn.lpstrInitialDir=root.c_str();
    ofn.Flags=OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST|OFN_HIDEREADONLY;
    ofn.lpstrDefExt="stk";
    BOOL res=GetOpenFileName(&ofn);
    if (!res) return;
  }
  char err[1000];
  bool res=LoadBody(&body,fn,err,lbForEditing);
  if (!res) {MessageBox(mhwnd,err,"Error opening file",MB_OK);return;}
  strcpy(curfile,fn); ismodified=false; ShowTitle();
  tool=tEdit;
  FileReady();
  string msg = "Opened file "+stk::ChangeFileExt(stk::ExtractFileName(fn),""); ShowHint(msg);
}

void TEditor::FileNew()
{ if (!FileClose()) return;
  body->NewFile();
  strcpy(curfile,""); ismodified=false;
  ShowJoints=true; ShowAngles=true; ShowInvisibles=true;
  tool=tCreate;
  FileReady();
  ShowHint("CREATE mode. To create a new line, click+drag on a joint.");
}



void TEditor::FileReady()
{ mode=mNothing; hit.n=-1; hit.s=-1; cori=-1; hit.type=htMiss;
  sellimbs.clear(); selshapes.clear();
  zoom=30; offx=0; offy=0;
  nextchan=0; nextband=0; nextlinestyle=""; nextfreqstyle=""; nextfillstyle="";
  for (list<TUndoData>::iterator i=undos.begin(); i!=undos.end(); i++) i->releasebmps();
  frees.splice(frees.end(),undos); undopos=-1;
  fnbackdrop=RecallBack(curfile); Backdrop(fnbackdrop,true);
  ismodified=false;
  isready=true;
  Recalc(0);
  ShowTitle();
  UpdateMenus();
  SetScrollBars();
  Redraw(); SetCursor();
}

bool TEditor::FileClose()
{ if (ismodified)
  { string s = "Do you want to save the changes you made to ";
    if (strcmp(curfile,"")==0) s+="[untitled]"; else s+=stk::ChangeFileExt(stk::ExtractFileName(curfile),"");;
    s+="?";
    int res=MessageBox(mhwnd,s.c_str(),"Sticky",MB_YESNOCANCEL);
    if (res==IDCANCEL) return false;
    if (res==IDYES)
    { bool bres=FileSave();
      if (!bres) return false;
    }
  }
  for (list<TUndoData>::iterator i=undos.begin(); i!=undos.end(); i++) i->releasebmps();
  frees.splice(frees.end(),undos); undopos=-1;
  return true;
}

typedef struct {int numrot; TEditor *editor;} TRotDlgDat;

int WINAPI UprightDlgProc(HWND hdlg,UINT msg,WPARAM wParam,LPARAM lParam)
{ TRotDlgDat *dat;
  #pragma warning( push ) 
  #pragma warning( disable : 4244 4312 )
  if (msg==WM_INITDIALOG) {SetWindowLongPtr(hdlg,DWLP_USER,lParam); dat=(TRotDlgDat*)lParam;}
  else {dat=(TRotDlgDat*)GetWindowLongPtr(hdlg,DWLP_USER);}
  #pragma warning( pop )
  if (dat==NULL) return FALSE;
  TEditor *editor = dat->editor;
  //
  switch (msg)
  { case WM_INITDIALOG:
    { CheckDlgButton(hdlg,IDC_ALWAYSCHECK,UserCheckUpright?BST_CHECKED:BST_UNCHECKED);
    } // and fall through to the rest of the stuff...
    case WM_APP:
    { char c[1000]; wsprintf(c,"The stick figure currently contains %i non-upright bitmaps.",dat->numrot);
      SetDlgItemText(hdlg,IDC_UPRIGHTREPORT,c);
      EnableWindow(GetDlgItem(hdlg,IDC_FIX),(dat->numrot==0)?FALSE:TRUE);
      return TRUE;
    }
    case WM_COMMAND:
    { int id=LOWORD(wParam), code=HIWORD(wParam);
      if (id==IDOK) UserCheckUpright=(IsDlgButtonChecked(hdlg,IDC_ALWAYSCHECK)==BST_CHECKED);
      if (id==IDOK || id==IDCANCEL) EndDialog(hdlg,id);
      if (id==IDC_FIX && code==BN_CLICKED)
      { editor->FixUpright();
        dat->numrot=0;
        SendMessage(hdlg,WM_APP,0,0);
      }
      return TRUE;
    }
  }
  return FALSE;
}


bool TEditor::MaybeCheckUpright(bool forcedialog)
{ if (!UserCheckUpright && !forcedialog) return true;
  int numrot=0;
  for (int si=0; si<(int)body->shapes.size(); si++)
  { int li=circlelimb_from_shape(si);
    if (body->shapes[si].brush.type==ctBitmap && li!=-1)
    { TLimb &limb = body->limbs[li];
      bool norot = (!limb.aisoff && limb.chan==4 && limb.band==0 && limb.aoff==0 && limb.aspring==0);
      if (!norot) numrot++;
    }
  }
  if (!forcedialog && numrot==0) return true;
  TRotDlgDat dat; dat.numrot=numrot; dat.editor=this;
  int res = DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_UPRIGHT),mhwnd,UprightDlgProc,(LPARAM)&dat);
  return (res==IDOK);
}

void TEditor::FixUpright()
{ MarkUndo();
  for (int si=0; si<(int)body->shapes.size(); si++)
  { int li=circlelimb_from_shape(si);
    if (body->shapes[si].brush.type==ctBitmap && li!=-1)
    { TLimb &limb = body->limbs[li];
      limb.aisoff=false;
      limb.aoff=0; limb.aspring=0;
      limb.chan=4; limb.band=0;
    }
  }
  ismodified=true;
  Recalc(0);
  Redraw(); SetCursor();
}



void TEditor::GetRootLimbsOfSellimbs(list<int> *roots)
{ vector<bool> sbitmap; sbitmap.resize(body->nlimbs);
  for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++) sbitmap[*i]=true;
  for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
  { int limb = *i; limb=body->limbs[limb].root;
    while (limb!=0 && !sbitmap[limb]) limb=body->limbs[limb].root;
    if (limb==0) roots->push_back(*i);
  }
}

bool TEditor::GetRootLimbsOfSelection(list<int> *roots)
{ // This routine is specifically designed for "copy". It gives an answer
  // that includes all selected limbs, and also all "feasible" shapes.
  // What is a feasible shape? -- well, intuitively, if you try to copy
  // a shape which spans two massive great trees, you shouldn't be able to,
  // since that would involve copying the two trees. But if you merely copy
  // a shape that's quite self-contained, then you should.
  // Thus: each shape i marks each of its limbs with a soft i.
  // No soft i can contribute more than once to the list of roots.
  // Therefore: if a shape is self-contained, it will have only one soft i root,
  // but if it's spread out it would have two or more,
  // but if it's spread out and the things were already hard-rooted, that'll be okay.
  if (roots!=NULL) roots->clear();
  if (selshapes.size()==0 && sellimbs.size()==1) {if (roots!=NULL) roots->push_back(sellimbs.front()); return true;}
  if (selshapes.size()==0 && sellimbs.size()==0) return false;
  //
  // First we mark the soft i's, and the hard i's with -1
  vector<int> bsel; bsel.resize(body->nlimbs,0);
  list<int> lsel;
  int ssi=0; for (list<int>::const_iterator i=selshapes.begin(); i!=selshapes.end(); i++,ssi++)
  { stk::TShape &shape = body->shapes[*i];
    for (vector<TJointRef>::const_iterator j=shape.p.begin(); j!=shape.p.end(); j++)
    { bsel[j->i]=ssi+1; lsel.push_back(j->i);
    }
  }
  for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
  { bsel[*i]=-1; lsel.push_back(*i);
  }
  lsel.sort(); lsel.unique();
  // now bsel is a bitmap of the selected limbs, and lsel is a list of the
  // limbs which we must account for.
  // Algorithm: for each limb we're intersted in, work towards the root, and mark
  // its rooties ancestor that was also in the list.
  vector<bool> gotsofts; gotsofts.resize(selshapes.size()+1,false);
  list<int> hroots;
  for (list<int>::const_iterator i=lsel.begin(); i!=lsel.end(); i++)
  { int limb=*i, aroot=limb;
    while (body->limbs[limb].root!=0)
    { limb=body->limbs[limb].root;
      if (bsel[limb]!=0) aroot=limb;
    }
    hroots.push_back(aroot);
    int type = bsel[aroot];
    LUASSERT(type!=0);
    if (type>0)
    { if (gotsofts[type]) return false; // too bad... this softi was already being used as a root elsewhere
      gotsofts[type]=true; bsel[aroot]=-1; // since we've opted for this, others can also use it
      // without them having to count against the softi budget as well.
    }
  }
  // done!
  hroots.sort(); hroots.unique();
  if (roots!=NULL) *roots = hroots;
  return true;
}


void TEditor::UpdateMenus()
{ //InvalidateRect(hdb,NULL,TRUE);
  HMENU hmenu = GetMenu(mhwnd);
  HMENU hed = GetSubMenu(hmenu,1);
  bool canstretch = (sellimbs.size()>0 || selshapes.size()==0); // either nothing, or some limbs
  EnableMenuItem(hed,ID_EDIT_ENLARGE,canstretch?MF_ENABLED:MF_GRAYED);
  EnableMenuItem(hed,ID_EDIT_SHRINK,canstretch?MF_ENABLED:MF_GRAYED);
  EnableMenuItem(hed,ID_EDIT_REDO,MF_BYCOMMAND|(undopos==-1?MF_GRAYED:MF_ENABLED));
  bool canundo=false; if (undopos==-1 && undos.size()>0) canundo=true;
  if (undopos!=-1 && undopos+2<(int)undos.size()) canundo=true; // if we've reached the end
  EnableMenuItem(hed,ID_EDIT_UNDO,MF_BYCOMMAND|(canundo?MF_ENABLED:MF_GRAYED));
  //int r = GetRootLimbOfSelection();
  bool cancopy = GetRootLimbsOfSelection(NULL);
  EnableMenuItem(hed,ID_EDIT_CUT,MF_BYCOMMAND|(cancopy?MF_ENABLED:MF_GRAYED));
  EnableMenuItem(hed,ID_EDIT_COPY,MF_BYCOMMAND|(cancopy?MF_ENABLED:MF_GRAYED));
  bool canpaste=false;
  if (sellimbs.size()==0 && selshapes.size()==0) canpaste=true; // paste to the root
  if (sellimbs.size()==1) canpaste=true;
  EnableMenuItem(hed,ID_EDIT_PASTE,MF_BYCOMMAND|(!canpaste?MF_GRAYED:MF_ENABLED));
  EnableMenuItem(hed,ID_EDIT_INSERT,MF_BYCOMMAND|(!canpaste?MF_GRAYED:MF_ENABLED));
  EnableMenuItem(hed,ID_EDIT_FLIP,MF_BYCOMMAND|(sellimbs.size()==0?MF_GRAYED:MF_ENABLED));
  MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi); mi.fMask=MIIM_STRING|MIIM_STATE; mi.fType=MFT_STRING;
  if (sellimbs.size()==0 && selshapes.size()==0) {mi.fState=MFS_DISABLED; mi.dwTypeData="&Delete line\tDel"; mi.cch=strlen(mi.dwTypeData);}
  else if (sellimbs.size()!=0 && selshapes.size()==0) {mi.fState=MFS_ENABLED; mi.dwTypeData="&Delete line\tDel"; mi.cch=strlen(mi.dwTypeData);}
  else if (sellimbs.size()==0 && selshapes.size()!=0) {mi.fState=MFS_ENABLED; mi.dwTypeData="&Delete shape\tDel"; mi.cch=strlen(mi.dwTypeData);}
  else if (sellimbs.size()!=0 && selshapes.size()!=0) {mi.fState=MFS_ENABLED; mi.dwTypeData="&Delete\tDel"; mi.cch=strlen(mi.dwTypeData);}
  SetMenuItemInfo(hed,ID_EDIT_DELETE,FALSE,&mi);
  HMENU ht  = GetSubMenu(hmenu,2);
  CheckMenuItem(ht,ID_TOOLS_TEST,MF_BYCOMMAND|(mode==mNothingTesting?MF_CHECKED:MF_UNCHECKED));
  CheckMenuItem(ht,ID_TOOLS_SHOWANGLES,MF_BYCOMMAND|(ShowAngles?MF_CHECKED:MF_UNCHECKED));
  CheckMenuItem(ht,ID_TOOLS_SHOWJOINTS,MF_BYCOMMAND|(ShowJoints?MF_CHECKED:MF_UNCHECKED));
  CheckMenuItem(ht,ID_TOOLS_SHOWINVISIBLES,MF_BYCOMMAND|(ShowInvisibles?MF_CHECKED:MF_UNCHECKED));
  CheckMenuItem(ht,ID_TOOLS_ZOOMMODE,MF_BYCOMMAND|(tool==tZoom?MF_CHECKED:MF_UNCHECKED));
  CheckMenuItem(ht,ID_TOOLS_EDITMODE,MF_BYCOMMAND|(tool==tEdit?MF_CHECKED:MF_UNCHECKED));
  CheckMenuItem(ht,ID_TOOLS_CREATEMODE,MF_BYCOMMAND|(tool==tCreate?MF_CHECKED:MF_UNCHECKED));
  CheckMenuItem(ht,ID_TOOLS_SNAP,MF_BYCOMMAND|(UserSnap?MF_CHECKED:MF_UNCHECKED));
  CheckMenuItem(ht,ID_TOOLS_GRID,MF_BYCOMMAND|(UserGrid?MF_CHECKED:MF_UNCHECKED));
  CheckMenuItem(ht,ID_INVERT,MF_BYCOMMAND|(WhitePrint?MF_CHECKED:MF_UNCHECKED));
  //
  ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi);
  mi.fMask=MIIM_DATA|MIIM_STATE|MIIM_TYPE; 
  mi.fType=MFT_STRING; const char *c="&Template"; if (hbackdrop==0) c="&Template...";
  mi.fState=MFS_ENABLED; if (hbackdrop!=0) mi.fState|=MFS_CHECKED;
  mi.dwTypeData=(char*)c; mi.cch=strlen(c);
  SetMenuItemInfo(ht,ID_TOOLS_BACKDROP,FALSE,&mi);
}

  

void im(HMENU hmenu,const string s,int id=0,bool checked=false,bool enabled=true,bool radio=true)
{ MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi); 
  if (s=="--")
  { mi.fMask=MIIM_TYPE|MIIM_ID;
    mi.fType=MFT_SEPARATOR; mi.wID=0;
    InsertMenuItem(hmenu,0,TRUE,&mi);
    return;
  }
  mi.fMask=MIIM_TYPE|MIIM_ID|MIIM_STATE|MIIM_DATA;
  mi.fType=MFT_STRING; if (radio) mi.fType|=MFT_RADIOCHECK;
  mi.wID = id;
  mi.fState=enabled?MFS_ENABLED:MFS_GRAYED; if (checked) mi.fState|=MFS_CHECKED;
  mi.dwTypeData = (char*)s.c_str(); mi.cch=s.length();
  InsertMenuItem(hmenu,0,TRUE,&mi);
}

void im(HMENU hmenu,const string s,HMENU hsubmenu)
{ MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi); 
  mi.fMask=MIIM_TYPE|MIIM_STATE|MIIM_ID|MIIM_DATA|MIIM_SUBMENU;
  mi.fType=MFT_STRING; 
  mi.fState=MFS_ENABLED;
  mi.wID = 0;
  mi.dwTypeData=(char*)s.c_str(); mi.cch=s.length();
  mi.hSubMenu = hsubmenu;
  InsertMenuItem(hmenu,0,TRUE,&mi);
}



void im(HMENU hmenu,TBody *body, int idfirst,bool keyboard,string selstyle)
{ MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi); 
  mi.fMask=MIIM_TYPE|MIIM_STATE|MIIM_ID|MIIM_DATA;
  mi.fType=MFT_STRING|MFT_RADIOCHECK;
  int id=(int)body->styles.size()-1;
  for (list<TStyle>::reverse_iterator i=body->styles.rbegin(); i!=body->styles.rend(); i++,id--)
  { string s=i->name;
    bool checked = (selstyle==s);
    if (keyboard && i->shortcut!=0) {s=s+"\tCtrl+"+string(&i->shortcut,1);}
    mi.wID = idfirst+id;
    mi.fState=MFS_ENABLED; if (checked) mi.fState|=MFS_CHECKED;
    mi.dwTypeData=(char*)s.c_str(); mi.cch=s.length();
    InsertMenuItem(hmenu,0,TRUE,&mi);
  }
}

enum im_effect_properties {imDefault=0, imDisallowBitmaps=1};
void im(HMENU hmenu,const vector<TEffect> &alleffects, const list<string> effects, bool notalleffect, int idfirst,im_effect_properties props=imDefault)
{ // effects is a list of the effects used in this selection.
  // notalleffect shows that some objects in the selection were not effects.
  // If they were all effects, and effects.size()==1, then tick it.
  MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi); 
  mi.fMask=MIIM_TYPE|MIIM_STATE|MIIM_ID|MIIM_DATA;
  mi.fType=MFT_STRING|MFT_RADIOCHECK;
  string def; if (effects.size()==1 && !notalleffect) def=effects.front();
  //
  int id=idfirst;
  for (vector<TEffect>::const_iterator i=alleffects.begin(); i!=alleffects.end(); i++, id++)
  { if (props==imDisallowBitmaps)
    { bool anybitmaps=false;
      for (vector<stk::TColor>::const_iterator j=i->cols.begin(); j!=i->cols.end(); j++)
      { if (j->type==ctBitmap) {anybitmaps=true; break;}
      }
      if (anybitmaps) continue;
    }
    mi.wID=id;
    mi.fState=MFS_ENABLED; if (StringLower(i->name)==StringLower(def)) mi.fState|=MFS_CHECKED;
    mi.dwTypeData=(char*)i->name.c_str(); mi.cch=i->name.length();
    InsertMenuItem(hmenu,0,TRUE,&mi);
  }
}


struct CumInfo
{ CumInfo(double _d, int _r) : d(_d),r(_r) {}
  double d; int r;
  bool operator< (const CumInfo &b) const {if (d<b.d) return true; if (d>b.d) return false; return r<b.r;}
  bool operator== (const CumInfo &b) const {return d==b.d && r==b.r;}
};

void im(HMENU hmenu,const list<CumInfo> cums,int idfirst,vector<CumInfo> &retcums)
{ // cums is the list of all the cums encountered in the selection.
  // The cums list is always expected to have something, even if just "0"
  // When we create the menu, we place a copy of it in retcums. And each item
  // is indexed to its position in retcums. That makes it easy for the caller to
  // retrieve the one that was chosen.
  MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi); 
  mi.fMask=MIIM_TYPE|MIIM_STATE|MIIM_ID|MIIM_DATA;
  mi.fType=MFT_STRING|MFT_RADIOCHECK;
  bool def=false; double ddef=0; int rdef=0;
  if (cums.size()==1) {def=true; ddef=cums.front().d; rdef=cums.front().r; rdef*=rdef;}
  // rdef may be -1 or 0 or 1. We convert to 0/1 here for convenience.
  int id=idfirst;
  retcums.clear();
  //
  // We will put into the menu all usercums. Then, as rate() or fixedrate(), anything
  // listed but not in usercums.
  for (list<CumInfo>::const_iterator j=cums.begin(); j!=cums.end(); j++)
  { double d=j->d; int dr=j->r; dr*=dr;
    bool wasinusercums=false;
    for (list<TUserCum>::const_iterator i=usercums.begin(); i!=usercums.end() && !wasinusercums; i++)
    { double aa=d, ab=i->r;
      bool ba=i->reflect, bb=(dr!=0);
      if (aa==ab && ((ba&&bb)||(!ba&&!bb))) wasinusercums=true;
    }
    if (wasinusercums) continue;
    string s; if (d<0 && dr==1) s="reflect_fixedrate("+StringFloat(-d)+")";
    else if (d<0) s="fixedrate("+StringFloat(-d)+")";
    else if (dr==1) s="reflect_rate("+StringFloat(d)+")";
    else s="rate("+StringFloat(d)+")";
    mi.wID = id;
    mi.fState=MFS_ENABLED; if (def && ddef==d && rdef==dr) mi.fState|=MFS_CHECKED;
    mi.dwTypeData=(char*)s.c_str(); mi.cch=s.length();
    InsertMenuItem(hmenu,0,TRUE,&mi);
    retcums.push_back(CumInfo(d,dr)); id++;
  }
  for (list<TUserCum>::reverse_iterator i=usercums.rbegin(); i!=usercums.rend(); i++)
  { string s = i->name; int ireflect=(i->reflect?1:0);
    mi.wID = id;
    mi.fState=MFS_ENABLED; if (def && ddef==i->r && rdef==ireflect) mi.fState|=MFS_CHECKED;
    mi.dwTypeData=(char*)s.c_str(); mi.cch=s.length();
    InsertMenuItem(hmenu,0,TRUE,&mi);
    retcums.push_back(CumInfo(i->r,ireflect)); id++;
  }
}



void im(HMENU hmenu,const list<COLORREF> cols,bool uncol, int idfirst,bool keyboard,vector<COLORREF> *extracols)
{ // cols is a list of all the colours encountered in the selection.
  // uncol is true if the selection also includes an object missing a colour:
  // this prevents any tickmark from appearing.
  // If cols has size 1, and uncol=false, then only one colour is used in the list, so check it!
  // when we create the list, we give indexes 0 <= i< ncols for the standard colours
  // and ncols <= i < ncols+usercols for the user-defined colours
  // and ncols+usercols <= i < ncols+usercols+extracols for the extra ones.
  // We also ensure that 'extracols' contains all these extra colours that we're talking about
  MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi); 
  mi.fMask=MIIM_TYPE|MIIM_STATE|MIIM_ID|MIIM_DATA;
  mi.fType=MFT_STRING|MFT_RADIOCHECK;
  // dr,dg,db indicate the colour that should have a checkmark on it
  int dr=-1,dg=-1,db=-1; if (cols.size()==1 && !uncol) {COLORREF c=cols.front(); dr=GetRValue(c); dg=GetGValue(c); db=GetBValue(c);}
  //
  // First we'll put in the extracols.
  for (list<COLORREF>::const_iterator i=cols.begin(); i!=cols.end(); i++)
  { int id=-1;
    int r=GetRValue(*i), g=GetGValue(*i), b=GetBValue(*i);
    for (int nc=0; nc<ncols && id==-1; nc++)
    { if (defcols[nc].r==r && defcols[nc].g==g && defcols[nc].b==b) id=nc;
    }
    int j=0;
    for (list<TUserColour>::const_iterator uc=usercols.begin(); uc!=usercols.end() && id==-1; uc++, j++)
    { if (uc->c.r==r && uc->c.g==g && uc->c.b==b) id=j+ncols; 
    }
    for (int ne=0; ne<(int)extracols->size() && id==-1; ne++)
    { COLORREF c=(*extracols)[ne];
      if (r==GetRValue(c) && g==GetGValue(c) && b==GetBValue(c)) id=j+ncols+usercols.size()+ne;
    }
    if (id==-1)
    { id=ncols+usercols.size()+extracols->size();
      extracols->push_back(RGB(r,g,b));
    }
    if (id>=ncols+(int)usercols.size())
    { string s = "RGB("+StringInt(r)+","+StringInt(g)+","+StringInt(b)+")";
      mi.wID = idfirst+id;
      mi.fState=MFS_ENABLED; if (dr==r && dg==g && db==b) mi.fState|=MFS_CHECKED;
      mi.dwTypeData=(char*)s.c_str(); mi.cch=s.length();
      InsertMenuItem(hmenu,0,TRUE,&mi);
    }
  }
  //
  // Now the usercols
  int j=usercols.size()-1;
  for (list<TUserColour>::reverse_iterator it=usercols.rbegin(); it!=usercols.rend(); it++,j--)
  { string s=it->name;
    int id=ncols+j;
    mi.wID = idfirst+id;
    mi.fState=MFS_ENABLED; if (dr==it->c.r && dg==it->c.g && db==it->c.b) mi.fState|=MFS_CHECKED;
    mi.dwTypeData=(char*)s.c_str(); mi.cch=s.length();
    InsertMenuItem(hmenu,0,TRUE,&mi);
  }
  //
  // Now the defcols
  char *colkey[2][7]={{"Black\tK","White\tW","Red\tD","Green\tG","Blue\tB","Yellow\tY","Purple\tP"},
                      {"Black","White","Red","Green","Blue","Yellow","Purple"}};
  for (j=ncols-1; j>=0; j--)
  { string s; if (keyboard) s=colkey[0][j]; else s=colkey[1][j];
    int id=j;
    mi.wID = idfirst+id;
    mi.fState=MFS_ENABLED; if (dr==defcols[j].r && dg==defcols[j].g && db==defcols[j].b) mi.fState|=MFS_CHECKED;
    mi.dwTypeData=(char*)s.c_str(); mi.cch=s.length();
    InsertMenuItem(hmenu,0,TRUE,&mi);
  }
}



void BalanceMenu(HMENU hmenu)
{ int n=GetMenuItemCount(hmenu);
  MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi));
  mi.cbSize=sizeof(mi); mi.fMask=MIIM_FTYPE;
  for (int i=38; i<n; i+=38)
  { GetMenuItemInfo(hmenu,i,TRUE,&mi);
    mi.fType|=MFT_MENUBARBREAK;
    SetMenuItemInfo(hmenu,i,TRUE,&mi);
  }
}



void TEditor::BackgroundClick(int mx,int my)
{ hit.n=-1; hit.s=-1; mode=mNothing; Redraw(); sellimbs.clear(); selshapes.clear();
  //
  enum {rmMoreColours=16, rmColourFirst=17, rmColourLast=99,
        rmMoreEffects=100,rmEffectsFirst=101, rmEffectsLast=199,
        rmZoom=200,rmEdit=201,rmCreate=202};
  HMENU hrightmenu=CreatePopupMenu(), hcolmenu=CreatePopupMenu();
  vector<COLORREF> extracols; TLimb &limb = body->limbs[0];

  // colour menu
  list<string> effects; if (limb.color.type==ctEffect) effects.push_back(limb.color.effect);
  im(hcolmenu,"Effects...",rmMoreEffects);
  im(hcolmenu,body->effects,effects,effects.size()==0,rmEffectsFirst,imDisallowBitmaps);
  im(hcolmenu,"--");
  list<COLORREF> cols; if (limb.color.type == ctRGB) cols.push_back(RGB(limb.color.rgb.r, limb.color.rgb.g, limb.color.rgb.b));
  im(hcolmenu,"Colours...",rmMoreColours);
  im(hcolmenu,cols,cols.size()==0,rmColourFirst,false,&extracols);

  // background menu
  im(hrightmenu,"Background",hcolmenu);
  im(hrightmenu,"--");
  im(hrightmenu,"Zoom",rmZoom,tool==tZoom);
  im(hrightmenu,"Edit",rmEdit,tool==tEdit);
  im(hrightmenu,"Create",rmCreate,tool==tCreate);

  POINT pt; pt.x=mx; pt.y=my; ClientToScreen(chwnd,&pt);
  int cmd=TrackPopupMenu(hrightmenu,TPM_LEFTALIGN|TPM_TOPALIGN|TPM_RETURNCMD,pt.x,pt.y,0,chwnd,NULL);
  DestroyMenu(hrightmenu);
  if (cmd==0) return;
  //
  if (cmd==rmZoom) {tool=tZoom; ShowHint("");}
  else if (cmd==rmEdit) {tool=tEdit; ShowHint("");}
  else if (cmd==rmCreate) {tool=tCreate; ShowHint("CREATE mode. Click and drag to create a new line out of a joint");}
  else if (cmd==rmMoreColours) UserColours(mhwnd);
  else if (cmd==rmMoreEffects) Effects(mhwnd);
  else if (cmd>=rmEffectsFirst && cmd<=rmEffectsLast)
  { MarkUndo();
    limb.color.type=ctEffect; limb.color.effect=body->effects[cmd-rmEffectsFirst].name;
    ismodified=true;
    MakeEindexes(body); body->RecalcEffects(true);
    Redraw();
  }
  else if (cmd>=rmColourFirst && cmd<=rmColourLast)
  { MarkUndo(); int r,g,b, c=cmd-rmColourFirst;
    if (c<ncols) {r=defcols[c].r; g=defcols[c].g; b=defcols[c].b;}
    else if (c<ncols+(int)usercols.size())
    { int ci=ncols; list<TUserColour>::const_iterator ui=usercols.begin();
      while (ci<c) {ci++; ui++;}
      r=ui->c.r; g=ui->c.g; b=ui->c.b;
    }
    else
    { int e=c-ncols-(int)usercols.size(); COLORREF cl=extracols[e];
      r=GetRValue(cl); g=GetGValue(cl); b=GetBValue(cl);
    }
	limb.color.rgb.r = r; limb.color.rgb.g = g; limb.color.rgb.b = b; limb.color.type = ctRGB; limb.color.dtype = ctRGB;
    WhitePrint=false;
    UpdateMenus();
    ismodified=true;
    Redraw();
  }

}



void TEditor::RightClick(int mx,int my)
{ THitTest ht=PointAtCoords(mx,my,true);
  if (ht.n<=0 && ht.s==-1) {BackgroundClick(mx,my); return;}
  // If a right-click on a selected thing, fine. But if a right-click on
  // an unselected thing, then the selection must be removed. Unless shift.
  bool isin=false; bool shift = (GetAsyncKeyState(VK_SHIFT)<0);
  if (ht.n>0)
  { for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end() && !isin; i++) {if (*i==ht.n) isin=true;}
  }
  else
  { for (list<int>::const_iterator i=selshapes.begin(); i!=selshapes.end() && !isin; i++) {if (*i==ht.s) isin=true;}
  }
  if (!isin && !shift)
  { sellimbs.clear(); selshapes.clear();
    if (ht.n>=0) {sellimbs.push_back(ht.n); int si=circleshape_from_limb(ht.n); if (si!=-1) selshapes.push_back(si);}
    else {selshapes.push_back(ht.s); int li=circlelimb_from_shape(ht.s); if (li!=-1) sellimbs.push_back(li);}
    hit=ht; mode=mNothing; Redraw();
  }
  else if (!isin && shift)
  { if (ht.n>=0) {sellimbs.push_back(ht.n); int si=circleshape_from_limb(ht.n); if (si!=-1) selshapes.push_back(si);}
    else {selshapes.push_back(ht.s); int li=circlelimb_from_shape(ht.s); if (li!=-1) sellimbs.push_back(li);}
    hit=ht; mode=mNothing; Redraw();
  }
  bool keybline=true;
  if (ht.n==-1 && ht.s!=-1 && ht.type==htShape) keybline=false;

  // We will do a general sort of right-click menu here, which works for
  // both shapes and limbs, an arbitrary number of them. Some options aren't
  // available in all cases.
  // line/arc/spring/circle - only if sellimb=1
  // --
  // Copy    }
  // Cut     } only if sellimbs>1
  // Paste   } additionally, paste requires that sellimb=1, and is grey otherwise
  // --      }
  // Insert Line  }
  // Delete Line  } only if 1 limb selected
  // --           }
  // Flip    }
  // Enlarge } only if 1 or more limbs selected
  // Shrink  }
  // --      }
  // Frequency -> (list) -- present if 1 or more limbs selected
  // Line -> invisible/thickness/(list) -- always there, refers to lines and shape outlines
  // Fill -> invisible/anti-overlap/(list)/(blist) -- present if shape>=1||circle>=1, refers to shape fills
  // Order -> tofront/toback  -- always there, keeps relative order
  // Anchor -> toleft/tobottom/toright/totop -- present if 1 or more limbs selected, applies to all of them

  enum {rmAnchNone=1, rmAnchLeft=2, rmAnchRight=3, rmAnchTop=4, rmAnchBottom=5,
        rmOrderFront=10, rmOrderBack=11,
        rmFillVisible=12, rmFillInvisible=13, rmFillAlternate=14, rmFillWinding=15, rmFillMore=16, rmFill=17, rmFillLast=49,
        rmLineVisible=50, rmLineInvisible=51, rmLineMore=52, rmLineExtra2=53, rmLineExtra1=54, rmLineThick=55, rmLineMedium=56, rmLineThin=57, rmLine=58, rmLineLast=99,
        rmFreqFixed=111, rmFreqNegative=113, rmFreqLeft=114, rmFreqRight=125, rmFreqDifference=135, rmFreqKaraoke=143,
        rmShrink=151, rmEnlarge=152, rmFlip=153, rmInsert=154, rmDelete=155,
        rmPaste=156, rmCut=157, rmCopy=158, rmType=159,
        rmMoreBitmaps=180, rmBitmaps=181, rmBitmapsLast=250,
        rmMoreStyles=251,
        rmLineStyleFirst=252,rmLineStyleLast=352,
        rmFreqStyleFirst=353,rmFreqStyleLast=453,
        rmFillStyleFirst=454,rmFillStyleLast=555,
        rmEffectsMore=556, rmLineEffectsFirst=557, rmLineEffectsLast=600, rmFillEffectsFirst=601, rmFillEffectsLast=699,
        rmFreqCumMore=700, rmFreqCumFirst=701, rmFreqCumLast=799};
    
  HMENU hrightmenu=CreatePopupMenu(), hordmenu=CreatePopupMenu(), hanchmenu=CreatePopupMenu();
  HMENU hlinemenu=CreatePopupMenu(), hfillmenu=CreatePopupMenu(), hfreqmenu=CreatePopupMenu();
  vector<COLORREF> extracols; 
  MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi); 

  //
  // Anchor -> toleft/tobottom/toright/totop -- present if 1 or more limbs selected, applies to all of them
  if (sellimbs.size()>0)
  { int ia=-2;
    for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
    { int a=body->limbs[*i].anchor;
      if (ia==-2) ia=a; else if (ia!=a) ia=-1;
    }
    im(hanchmenu,"To left",rmAnchLeft,ia==4);
    im(hanchmenu,"To bottom",rmAnchBottom,ia==3);
    im(hanchmenu,"To right",rmAnchRight,ia==2);
    im(hanchmenu,"To top",rmAnchTop,ia==1);
    im(hanchmenu,"None",rmAnchNone,ia==0);
    im(hrightmenu,"Anchor",hanchmenu);
  }
  //
  // Order -> tofront/toback  -- always there, keeps relative order
  im(hordmenu,"To back\tEnd",rmOrderBack);
  im(hordmenu,"To front\tHome",rmOrderFront);
  im(hrightmenu,"Order",hordmenu);
  //
  // Fill -> none/(list) -- present if shape>=1||circles>=1, refers to shape fills
  bool fillable = (selshapes.size()>0);
  if (!fillable)
  { for (list<int>::const_iterator sli=sellimbs.begin(); sli!=sellimbs.end() && !fillable; sli++)
    { if (body->limbs[*sli].type==3) fillable=true;
    }
  }
  if (fillable)
  { list<COLORREF> cols; bool somenotrgb=false;
    list<string> effects; bool somenoteffects=false;
    list<string> bitmaps; bool somenotbitmaps=false;
    bool allinvisible=true, allalternate=true; string allfillstyle="==";
    //
    for (list<int>::const_iterator i=selshapes.begin(); i!=selshapes.end(); i++)
    { stk::TShape &shape = body->shapes[*i];
      string thisstyle=shape.fillstyle;
      if (allfillstyle=="==") allfillstyle=thisstyle;
      else if (allfillstyle!=thisstyle) allfillstyle="=";
      //
      if (!shape.balternate) allalternate=false;
      //
      if (shape.brush.type!=ctNone) allinvisible=false;
      if (shape.brush.type==ctBitmap) bitmaps.push_back(shape.brush.bitmap);
      else somenotbitmaps=true;
	  if (shape.brush.type == ctRGB) cols.push_back(RGB(shape.brush.rgb.r, shape.brush.rgb.g, shape.brush.rgb.b));
      else somenotrgb=true;
      if (shape.brush.type==ctEffect) effects.push_back(shape.brush.effect);
      else somenoteffects=true;
    }
    cols.sort(); cols.unique(); bitmaps.sort(); bitmaps.unique(); effects.sort(); effects.unique();
    //
    im(hfillmenu,"Styles...",rmMoreStyles);
    im(hfillmenu,body,rmFillStyleFirst,ht.n==-1 && !keybline,allfillstyle);
    im(hfillmenu,"--");
    im(hfillmenu,"Effects...",rmEffectsMore);
    im(hfillmenu,body->effects,effects,somenoteffects,rmFillEffectsFirst);
    im(hfillmenu,"--");
    string bitmapsel="-"; if (bitmaps.size()==1 && !somenotbitmaps) bitmapsel=bitmaps.front();
    im(hfillmenu,"Bitmaps...",rmMoreBitmaps);
    for (int bic=0; bic<(int)body->bmps.size(); bic++)
    { string bname = body->bmps[bic].name;
      bool checked = (StringLower(bname)==StringLower(bitmapsel));
      im(hfillmenu,bname,rmBitmaps+bic,checked,true,true);
    }
    im(hfillmenu,"--");
    im(hfillmenu,"Colours...",rmFillMore);
    im(hfillmenu,cols,somenotrgb,rmFill,!keybline,&extracols);
    im(hfillmenu,"--");
    im(hfillmenu,"Self-inverse",allalternate?rmFillWinding:rmFillAlternate,allalternate,true,false);
    string s="Invisible"; if (!keybline) s+="\tV";
    im(hfillmenu,s,allinvisible?rmFillVisible:rmFillInvisible,allinvisible,true,false);
    im(hrightmenu,"Fill",hfillmenu);
  }

  // Line -> invisible/thickness/(list) -- always there, refers to lines and shape outlines
  list<COLORREF> cols; bool notallrgb=false;
  list<string> lineeffects; bool notalleffects=false;
  bool allinvisible=true; int thick=-2;
  string alllinestyle("==");
  for (list<int>::const_iterator sli=sellimbs.begin(); sli!=sellimbs.end(); sli++)
  { TLimb &limb = body->limbs[*sli];
    bool is_a_shaped_circle =  (limb.type==3 && circleshape_from_limb(*sli)!=-1);
    // we ignore a limb-circle if it has a shape, for populating the line menu
    if (!is_a_shaped_circle)
	{ if (limb.color.type == ctRGB) { cols.push_back(RGB(limb.color.rgb.r, limb.color.rgb.g, limb.color.rgb.b)); allinvisible = false; }
      else notallrgb=true;
      if (limb.color.type==ctEffect) {lineeffects.push_back(limb.color.effect); allinvisible=false;}
      else notalleffects=false;
      //
      int it=0; if (limb.thickness<0.5) it=0; else if (limb.thickness<1.5) it=1;
      else if (limb.thickness<3) it=2; else if (limb.thickness<6) it=3; else it=4;
      if (limb.color.type==ctNone) thick=-1;
      if (thick==-2) thick=it; else if (thick!=it) thick=-1;
      //
      string thisstyle=limb.linestyle;
      if (alllinestyle=="==") alllinestyle=thisstyle;
      else if (alllinestyle!=thisstyle) alllinestyle="=";
    }
  }
  for (list<int>::const_iterator ssi=selshapes.begin(); ssi!=selshapes.end(); ssi++)
  { stk::TShape &shape = body->shapes[*ssi];
    if (shape.line.type==ctRGB) {cols.push_back(CREF(shape.line.rgb)); allinvisible=false;}
    else notallrgb=true;
    if (shape.line.type==ctEffect) {lineeffects.push_back(shape.line.effect); allinvisible=false;}
    else notalleffects=true;
    if (shape.line.type!=ctNone && shape.line.type!=ctRGB && shape.line.type!=ctEffect) LUASSERTMM("unknown shape color");
    //
    int it=0; if (shape.thickness<0.5) it=0; else if (shape.thickness<1.5) it=1; else it=2;
    if (shape.line.type==ctNone) thick=-1;
    if (thick==-2) thick=it; else if (thick!=it) thick=-1;
    //
    string thisstyle=shape.linestyle;
    if (alllinestyle=="==") alllinestyle=thisstyle;
    else if (alllinestyle!=thisstyle) alllinestyle="=";
  }
  cols.sort(); cols.unique();
  lineeffects.sort(); lineeffects.unique();
  //
  im(hlinemenu,"Styles...",rmMoreStyles);
  im(hlinemenu,body,rmLineStyleFirst,ht.n!=-1 || keybline,alllinestyle);
  im(hlinemenu,"--");
  im(hlinemenu,"Effects...",rmEffectsMore);
  im(hlinemenu,body->effects,lineeffects,notalleffects,rmLineEffectsFirst,imDisallowBitmaps);
  im(hlinemenu,"--");
  im(hlinemenu,"Colours...",rmLineMore);
  im(hlinemenu,cols,notallrgb,rmLine,keybline,&extracols);
  im(hlinemenu,"--");
  im(hlinemenu,"Extra2",rmLineExtra2,thick==4);
  im(hlinemenu,"Extra1",rmLineExtra1,thick==3);
  im(hlinemenu,"Heavy\tH",rmLineThick,thick==2);
  im(hlinemenu,"Medium\tM",rmLineMedium,thick==1);
  im(hlinemenu,"Thin\tT",rmLineThin,thick==0);
  im(hlinemenu,"--");
  string s="Invisible"; if (keybline) s+="\tV";
  im(hlinemenu,s,allinvisible?rmLineVisible:rmLineInvisible,allinvisible,true,false);
  im(hrightmenu,"Line",hlinemenu);
  //
  // Frequency -> (list) -- present if 1 or more limbs selected
  vector<CumInfo> retcums; // this will be constructed by the cumulative menu
  if (sellimbs.size()>0)
  { int chan=-2, band=-2, neg=-2; //-2=unassinged, -1=contradiction, 0+=value
    list<CumInfo> cums; // lists all the cums that we've seen so far
    bool anynonfixed=false; // for whether any have a chan=0,1,2 (and so can be negated and cum'd)
    string allfreqstyle("==");
    for (list<int>::const_iterator sli=sellimbs.begin(); sli!=sellimbs.end(); sli++)
    { TLimb &limb = body->limbs[*sli];
      int c=limb.chan, b=limb.band; int n=0; if (limb.negative&&c!=4) n=1;
      if (chan==-2) chan=c; else if (chan!=c) chan=-1;
      if (band==-2) band=b; else if (band!=b) band=-1;
      if (limb.chan<=3)
      { anynonfixed=true;
        if (neg==-2) neg=n; else if (neg!=n) neg=-1;
        if (limb.cum) cums.push_back(CumInfo(limb.crate,limb.creflect)); else cums.push_back(CumInfo(0,0));
      }
      //
      string thisstyle=limb.freqstyle;
      if (allfreqstyle=="==") allfreqstyle=thisstyle;
      else if (allfreqstyle!=thisstyle) allfreqstyle="=";
    }
    cums.sort(); cums.unique();
    //
    im(hfreqmenu,"Styles...",rmMoreStyles);
    im(hfreqmenu,body,rmFreqStyleFirst,false,allfreqstyle);
    if (anynonfixed)
    { im(hfreqmenu,"--");
      im(hfreqmenu,"Cumulatives...",rmFreqCumMore);
      im(hfreqmenu,cums,rmFreqCumFirst,retcums);
      im(hfreqmenu,"Negative",rmFreqNegative,neg==1,true,false);
    }
    im(hfreqmenu,"--");
    im(hfreqmenu,"Music", rmFreqKaraoke+1, chan==3&&band==1);
    im(hfreqmenu,"Vocals",rmFreqKaraoke+0, chan==3&&band==0);
    im(hfreqmenu,"--");
    im(hfreqmenu,"Difference: 6",rmFreqDifference+5, chan==2&&band==5);
    im(hfreqmenu,"Difference: 5",rmFreqDifference+4, chan==2&&band==4);
    im(hfreqmenu,"Difference: 4",rmFreqDifference+3, chan==2&&band==3);
    im(hfreqmenu,"Difference: 3",rmFreqDifference+2, chan==2&&band==2);
    im(hfreqmenu,"Difference: 2",rmFreqDifference+1, chan==2&&band==1);
    im(hfreqmenu,"Difference: 1",rmFreqDifference+0, chan==2&&band==0);
    im(hfreqmenu,"--");
    im(hfreqmenu,"Right: 6 treble",rmFreqRight+5, chan==1&&band==5);
    im(hfreqmenu,"Right: 5",rmFreqRight+4, chan==1&&band==4);
    im(hfreqmenu,"Right: 4",rmFreqRight+3, chan==1&&band==3);
    im(hfreqmenu,"Right: 3",rmFreqRight+2, chan==1&&band==2);
    im(hfreqmenu,"Right: 2",rmFreqRight+1, chan==1&&band==1);
    im(hfreqmenu,"Right: 1 bass\tR",rmFreqRight+0, chan==1&&band==0);
    im(hfreqmenu,"--");
    im(hfreqmenu,"Left: 6 treble",rmFreqLeft+5, chan==0&&band==5);
    im(hfreqmenu,"Left: 5",rmFreqLeft+4, chan==0&&band==4);
    im(hfreqmenu,"Left: 4",rmFreqLeft+3, chan==0&&band==3);
    im(hfreqmenu,"Left: 3",rmFreqLeft+2, chan==0&&band==2);
    im(hfreqmenu,"Left: 2",rmFreqLeft+1, chan==0&&band==1);
    im(hfreqmenu,"Left: 1 bass\tL",rmFreqLeft+0, chan==0&&band==0);
    im(hfreqmenu,"--");
    im(hfreqmenu,"Fixed\tF",rmFreqFixed, chan==4&&band==0);
    im(hrightmenu,"Frequency",hfreqmenu);
  }
  // Flip    }
  // Enlarge } only if 1 or more limbs selected
  // Shrink  }
  // --      }
  if (sellimbs.size()>0)
  { im(hrightmenu,"--");
    im(hrightmenu,"Flip",rmFlip);
    im(hrightmenu,"Enlarge",rmEnlarge);
    im(hrightmenu,"Shrink",rmShrink);
  }
  // Insert Line  }
  // Delete Line  } only if 1 limb selected
  // --           }
  if (sellimbs.size()==1)
  { im(hrightmenu,"--");
    im(hrightmenu,"Delete Line",rmDelete);
    im(hrightmenu,"Insert Line",rmInsert);
  }
  // Copy    }
  // Cut     } only if sellimbs>1
  // Paste   } additionally, paste requires that sellimb=1, and is grey otherwise
  // --      }
  bool cancopy = GetRootLimbsOfSelection(NULL);
  if (cancopy)
  { im(hrightmenu,"--");
    im(hrightmenu,"Paste",rmPaste,false,sellimbs.size()==1);
    im(hrightmenu,"Cut",rmCut);
    im(hrightmenu,"Copy",rmCopy);
  }
  // line/arc/spring/circle - only if sellimb=1
  // --
  if (sellimbs.size()==1)
  { im(hrightmenu,"--");
    TLimb &limb = body->limbs[sellimbs.front()];
    if (limb.type==0) im(hrightmenu,"Arc\tA",rmType);
    else if (limb.type==1) im(hrightmenu,"Spring\tA",rmType);
    else if (limb.type==2) im(hrightmenu,"Circle\tA",rmType);
    else im(hrightmenu,"Line\tA",rmType);
  }

  BalanceMenu(hrightmenu); BalanceMenu(hfreqmenu);
  BalanceMenu(hlinemenu); BalanceMenu(hfillmenu);
    
  POINT pt; pt.x=mx; pt.y=my; ClientToScreen(chwnd,&pt);
  int cmd=TrackPopupMenu(hrightmenu,TPM_LEFTALIGN|TPM_TOPALIGN|TPM_RETURNCMD,pt.x,pt.y,0,chwnd,NULL);
  DestroyMenu(hrightmenu);
  DestroyMenu(hanchmenu);
  DestroyMenu(hfreqmenu);
  DestroyMenu(hlinemenu);
  DestroyMenu(hfillmenu);
  DestroyMenu(hordmenu);
  mode=mNothing;
  if (cmd==0) return;

  //
 
  if (cmd==rmLineVisible) SetVisibility(tsLine,1);
  else if (cmd==rmLineInvisible) SetVisibility(tsLine,0);
  else if (cmd==rmFillVisible) {LazyCreateCircleShapes(); SetVisibility(tsFill,1);}
  else if (cmd==rmFillInvisible) SetVisibility(tsFill,0);
  else if (cmd==rmFillAlternate) SetAlternate(1);
  else if (cmd==rmFillWinding) SetAlternate(0);
  else if (cmd==rmType) ToggleLineType();
  else if (cmd==rmDelete) DeleteLimbOrShape();
  else if (cmd==rmInsert) InsertLimb();
  else if (cmd==rmCopy) Copy();
  else if (cmd==rmCut) Cut();
  else if (cmd==rmPaste) Paste();
  else if (cmd==rmEnlarge) Stretch(1);
  else if (cmd==rmShrink) Stretch(-1);
  else if (cmd==rmFillMore||cmd==rmLineMore) UserColours(mhwnd);
  else if (cmd==rmMoreBitmaps) Bitmaps(mhwnd);
  else if (cmd>=rmBitmaps && cmd<=rmBitmapsLast) {LazyCreateCircleShapes(); SetBitmap(body->bmps[cmd-rmBitmaps].name);}
  else if (cmd==rmFlip) Flip();
  else if (cmd==rmOrderFront) SetOrder(2);
  else if (cmd==rmOrderBack) SetOrder(-2);
  else if (cmd==rmFreqNegative) ToggleNegative();
  else if (cmd>=rmFreqDifference && cmd<=rmFreqDifference+5) SetChanBand(2,cmd-rmFreqDifference);
  else if (cmd>=rmFreqLeft && cmd<=rmFreqLeft+5) SetChanBand(0,cmd-rmFreqLeft);
  else if (cmd>=rmFreqRight && cmd<=rmFreqRight+5) SetChanBand(1,cmd-rmFreqRight);
  else if (cmd>=rmFreqKaraoke && cmd<=rmFreqKaraoke+5) SetChanBand(3,cmd-rmFreqKaraoke);
  else if (cmd==rmFreqFixed) SetChanBand(4,0);
  else if (cmd>=rmFreqCumFirst && cmd<=rmFreqCumLast) SetCum(retcums[cmd-rmFreqCumFirst].d,retcums[cmd-rmFreqCumFirst].r);
  else if (cmd==rmFreqCumMore) UserCumulatives(mhwnd);
  else if (cmd==rmAnchNone) SetAnchor(0);
  else if (cmd==rmAnchLeft) SetAnchor(4);
  else if (cmd==rmAnchRight) SetAnchor(2);
  else if (cmd==rmAnchBottom) SetAnchor(3);
  else if (cmd==rmAnchTop) SetAnchor(1);
  else if (cmd==rmLineExtra2) SetThickness(8.0);
  else if (cmd==rmLineExtra1) SetThickness(4.0);
  else if (cmd==rmLineThick) SetThickness(2.0);
  else if (cmd==rmLineMedium) SetThickness(1.0);
  else if (cmd==rmLineThin) SetThickness(0.0);
  else if ((cmd>=rmLine && cmd<=rmLineLast) || (cmd>=rmFill && cmd<=rmFillLast))
  { TSetSubject subj=tsLine; int c=cmd-rmLine;
    if (cmd>=rmFill && cmd<=rmFillLast) {subj=tsFill; c=cmd-rmFill;}
    if (subj==tsFill) LazyCreateCircleShapes();
    if (c<ncols+(int)usercols.size()) SetCol(subj,(TColIndex)c);
    else
    { int e=c-ncols-(int)usercols.size(); COLORREF r=extracols[e];
      SetCol(subj,GetRValue(r),GetGValue(r),GetBValue(r));
    }
  }
  else if (cmd>=rmLineEffectsFirst && cmd<=rmLineEffectsLast) SetEffect(tsLine,body->effects[cmd-rmLineEffectsFirst].name);
  else if (cmd>=rmFillEffectsFirst && cmd<=rmFillEffectsLast)
  { bool anybitmaps=false; const TEffect &eff = body->effects[cmd-rmFillEffectsFirst];
    for (vector<stk::TColor>::const_iterator i=eff.cols.begin(); i!=eff.cols.end(); i++)
    { if (i->type==ctBitmap) {anybitmaps=true; break;}
    }
    if (anybitmaps) LazyCreateCircleShapes(); 
    SetEffect(tsFill,body->effects[cmd-rmFillEffectsFirst].name);
  }
  else if (cmd==rmEffectsMore) Effects(mhwnd);
  else if (cmd==rmMoreStyles) Styles(mhwnd);
  else if (cmd>=rmLineStyleFirst && cmd<=rmLineStyleLast) SetStyle(tsLine,cmd-rmLineStyleFirst);
  else if (cmd>=rmFillStyleFirst && cmd<=rmFillStyleLast) {LazyCreateCircleShapes(); SetStyle(tsFill,cmd-rmFillStyleFirst);}
  else if (cmd>=rmFreqStyleFirst && cmd<=rmFreqStyleLast)
  { SetStyle(tsFreq,cmd-rmFreqStyleFirst);
  }
}



void TEditor::LazyCreateCircleShapes()
{ // This is because we display a fill menu for any circle limbs that
  // are selected -- even if they don't yet have a shape.
  // This function is called when a fill operation is applied.
  // We have to create any needed shapes, and add them to the selection.
  for (list<int>::const_iterator sli=sellimbs.begin(); sli!=sellimbs.end(); sli++)
  { TLimb &limb = body->limbs[*sli];
    bool iscircle = (limb.type==3);
    if (!iscircle) continue;
    int si = circleshape_from_limb(*sli);
    if (si!=-1) continue; // i.e. if it already had a circle shape, we're okay
    stk::TShape s; s.limbs=false; s.balternate=false;
    s.brush.type=ctRGB; s.brush.dtype=ctRGB; s.brush.rgb=RGBCOLOR(defcols[cYellow].r,defcols[cYellow].g,defcols[cYellow].b);
    s.line=limb.color; s.thickness=limb.thickness; limb.color.type=ctNone;
    TJointRef j; j.i=*sli; j.ar=true; s.p.push_back(j); j.ar=false; s.p.push_back(j);
    si=(int)body->shapes.size();
    body->shapes.push_back(s);
    selshapes.push_back(si);
  }
}


  
void TEditor::ToggleLineType()
{ if (sellimbs.size()==0) return;
  MarkUndo();
  for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
  { TLimb &limb=body->limbs[*i];
    bool wasspring = (limb.type==2||limb.type==3);
    limb.type = (limb.type+1)%4;
    bool isspring = (limb.type==2||limb.type==3);
    if (wasspring && !isspring)
    { // adjust ascale/f to match aspring.
      if (limb.chan==4 && limb.band==0)
      { limb.aspring=limb.f;
      }
      else
      { if (limb.ascale>0)
        { while (limb.aspring<0) limb.aspring+=2*pi; while (limb.aspring>2*pi) limb.aspring-=2*pi;
          if (limb.aspring>limb.ascale) {limb.ascale=limb.aspring*1.3; if (limb.ascale>2*pi) limb.ascale=2*pi;}
          limb.f=limb.aspring/limb.ascale;
        }
        else
        { while (limb.aspring<-2*pi) limb.aspring+=2*pi; while (limb.aspring>0) limb.aspring-=2*pi;
          if (limb.aspring<limb.ascale) {limb.ascale=limb.aspring*1.3; if (limb.ascale<-2*pi) limb.ascale=-2*pi;}
          limb.f=limb.aspring/limb.ascale;
        }
      }
    }
    else if (!wasspring && isspring)
    { // adjust aspring to match ascale/f
      if (limb.chan==4 && limb.band==0) limb.aspring=limb.f;
      else limb.aspring = limb.f*limb.ascale;
      // length is by definition fine: because an angled line has 'length',
      // and a spring goes in the range 'lmin->length'. So we just need to fix up lmin
      if (limb.lmin>limb.length) limb.lmin=limb.length*0.5;
      
    }
  }
  if (sellimbs.size()==1) Recalc(sellimbs.front());
  else Recalc(0);
  ismodified=true;
  Redraw(); SetCursor();
}

void TEditor::ToggleNegative()
{ nextfreqstyle="";
  if (sellimbs.size()==0) return;
  MarkUndo();
  for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
  { TLimb &limb = body->limbs[*i];
    if (limb.chan!=4) limb.negative=!limb.negative;
    // (can't have negative on a fixed limb)
    limb.freqstyle="";
  }
  ismodified=true;
  Redraw(); SetCursor();
}

void TEditor::SetAlternate(int newa)
{ if (selshapes.size()==0) return;
  MarkUndo();
  for (list<int>::const_iterator i=selshapes.begin(); i!=selshapes.end(); i++)
  { stk::TShape &shape = body->shapes[*i];
    if (newa==0) shape.balternate=false;
    else if (newa==1) shape.balternate=true;
    else shape.balternate = !shape.balternate;
  }
  ismodified=true;
  Redraw();
}

void TEditor::SetVisibility(TSetSubject subj,int newv)
{ if (subj==tsWhatever)
  { // tsWhatever means that it will be line or fill, according to which one
    // the user clicked on most recently to select.
    subj=tsLine;
    if (hit.n==-1 && hit.s!=-1 && hit.type==htShape) subj=tsFill;
  }
  if (subj==tsLineFill || subj==tsFill) nextfillstyle="";
  if (subj==tsLine || subj==tsLineFill) nextlinestyle="";
  if (sellimbs.size()==0 && selshapes.size()==0) return;
  MarkUndo();
  //
  if (subj==tsLine || subj==tsLineFill)
  { for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
    { TLimb &limb = body->limbs[*i];
      if (circleshape_from_limb(*i)==-1)
      { if (newv==0) {if (limb.color.type!=ctNone) limb.color.otype=limb.color.type; limb.color.type=ctNone;}
        else if (newv==1) limb.color.type=limb.color.otype;
        else {if (limb.color.type==ctNone) limb.color.type=limb.color.otype; else {limb.color.otype=limb.color.type; limb.color.type=ctNone;}}
        limb.linestyle="";
      }
      else {limb.linestyle=""; limb.color.type=ctNone;}
    }
  }
  for (list<int>::const_iterator i=selshapes.begin(); i!=selshapes.end(); i++)
  { stk::TShape &shape = body->shapes[*i];
    if (subj==tsLine || subj==tsLineFill)
    { if (newv==0) {if (shape.line.type!=ctNone) shape.line.otype=shape.line.type; shape.line.type=ctNone;}
      else if (newv==1) shape.line.type=shape.line.otype;
      else {if (shape.line.type==ctNone) shape.line.type=shape.line.otype; else {shape.line.otype=shape.line.type; shape.line.type=ctNone;}}
      shape.linestyle="";
    }
    if (subj==tsFill || subj==tsLineFill)
    { if (newv==0) {if (shape.brush.type!=ctNone) shape.brush.otype=shape.brush.type; shape.brush.type=ctNone;}
      else if (newv==1) shape.brush.type=shape.brush.otype;
      else {if (shape.brush.type==ctNone) shape.brush.type=shape.brush.otype; else {shape.brush.otype=shape.brush.type; shape.brush.type=ctNone;}}
      shape.fillstyle="";
    }
  }
  MakeBindexes(body); MakeEindexes(body); body->RecalcEffects(true);
  ismodified=true;
  Redraw(); SetCursor();
}

void TEditor::SetAnchor(int anch)
{ if (sellimbs.size()==0) return;
  MarkUndo();
  for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
  { TLimb &limb = body->limbs[*i];
    limb.anchor = anch;
  }
  ismodified=true;
  Redraw(); SetCursor();
}

void TEditor::SetCum(double c,int reflect)
{ if (sellimbs.size()==0) return;
  MarkUndo();
  for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
  { TLimb &limb = body->limbs[*i];
    if (!(limb.chan<=3)) continue;
    if (c==0) limb.cum=false;
    else {limb.cum=true; limb.crate=c; limb.creflect=reflect;}
    limb.freqstyle="";
  }
  ismodified=true;
}

void TEditor::SetChanBand(int ac,int ab)
{ nextchan=ac; nextband=ab; IncNextChan(); nextfreqstyle="";
  if (sellimbs.size()==0) return;
  MarkUndo();
  for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
  { TLimb &limb=body->limbs[*i];
    int c=ac,b=ab;
    if (b==-1) b=limb.band;
    if (c==-1) c=limb.chan;
    limb.band=b;
    limb.chan=c;
    RepositioningMove(*i,b2x(limb.x),b2y(limb.y),true);
    limb.freqstyle="";
  }
  ismodified=true;
  Recalc(0);
  Redraw(); SetCursor();
}

void TEditor::SetStyle(char shortcutkey)
{ int i=0;
  for (list<TStyle>::const_iterator si=body->styles.begin(); si!=body->styles.end(); si++,i++)
  { if (si->shortcut==shortcutkey) {SetStyle(tsWhatever,i); return;}
  }
  return;
}

void TEditor::SetStyle(TSetSubject subj, int index)
{ list<TStyle>::const_iterator si=body->styles.begin();
  for (int i=0; i<index; i++) si++;
  const TStyle &style = *si;
  if (subj==tsWhatever)
  { subj=tsLine;
    if (hit.n==-1 && hit.s!=-1 && hit.type==htShape) subj=tsFill;
    if (hit.n==-1 && hit.s==-1) {nextfreqstyle=style.name; nextlinestyle=style.name; nextfillstyle=style.name;}
    else if (hit.n!=-1) {nextfreqstyle=style.name; nextlinestyle=style.name;}
    else {nextfillstyle=style.name;}
  }
  else if (subj==tsFreq) nextfreqstyle=style.name;
  else if (subj==tsLine) nextlinestyle=style.name;
  else if (subj==tsFill) nextfillstyle=style.name;
  MarkUndo();
  bool anybmps=false;
  for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
  { TLimb &limb = body->limbs[*i];
    if (subj==tsFreq)
    { limb.freqstyle=style.name;
      limb.chan=style.limb.chan; limb.band=style.limb.band; limb.negative=style.limb.negative;
      limb.cum=style.limb.cum; limb.crate=style.limb.crate; limb.creflect=style.limb.creflect;
      RepositioningMove(*i,b2x(limb.x),b2y(limb.y),true);
    }
    if (subj==tsLine && circleshape_from_limb(*i)==-1)
    { limb.linestyle=style.name;
      limb.color=style.limb.color;
      limb.thickness=style.limb.thickness;
    }
  }
  for (list<int>::const_iterator i=selshapes.begin(); i!=selshapes.end(); i++)
  { stk::TShape &shape = body->shapes[*i];
    if (subj==tsFill) 
    { shape.fillstyle=style.name;
      shape.brush=style.shape.brush;
      anybmps=true;
    }
    if (subj==tsLine)
    { shape.linestyle=style.name;
      shape.line=style.limb.color;
      shape.thickness=style.limb.thickness;
    }
  }
  ismodified=true;
  if (anybmps) MakeBindexes(body);
  MakeEindexes(body); Recalc(0);
  Redraw(); SetCursor();
}


void TEditor::SetCol(TSetSubject subj, TColIndex i)
{ if (i<ncols) SetCol(subj,defcols[i].r,defcols[i].g,defcols[i].b);
  else
  { int ci=ncols; list<TUserColour>::const_iterator ui=usercols.begin();
    while (ci<i) {ci++; ui++;}
    SetCol(subj,ui->c.r,ui->c.g,ui->c.b);
  }
}

void TEditor::SetCol(TSetSubject subj,int r,int g,int b)
{ if (subj==tsWhatever)
  { subj=tsLine;
    if (hit.n==-1 && hit.s!=-1 && hit.type==htShape) subj=tsFill;
  }
  if (subj==tsLine || subj==tsLineFill || subj==tsFill) nextlinestyle="";
  if (subj==tsLineFill || subj==tsFill) nextfillstyle="";
  if (sellimbs.size()==0 && selshapes.size()==0) return;
  MarkUndo();
  //
  if (subj==tsLine || subj==tsLineFill)
  { for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
    { TLimb &limb = body->limbs[*i];
      if (circleshape_from_limb(*i)==-1)
      { limb.color.type=ctRGB; limb.color.dtype=ctRGB; limb.color.rgb=RGBCOLOR(r,g,b);
        limb.linestyle="";
      }
    }
  }
  for (list<int>::const_iterator i=selshapes.begin(); i!=selshapes.end(); i++)
  { stk::TShape &shape = body->shapes[*i];
    if (subj==tsLine || subj==tsLineFill)
    { shape.line.type=ctRGB; shape.line.dtype=ctRGB; shape.line.rgb=RGBCOLOR(r,g,b);
      shape.linestyle="";
    }
    if (subj==tsFill || subj==tsLineFill)
    { shape.brush.type=ctRGB; shape.brush.dtype=ctRGB; shape.brush.rgb=RGBCOLOR(r,g,b);
      shape.brush.bindex=-1;
      shape.fillstyle="";
    }
  }
  ismodified=true; body->RecalcEffects(true);
  Redraw(); SetCursor();
}


void TEditor::SetEffect(TSetSubject subj,const string effect)
{ if (subj==tsWhatever)
  { subj=tsLine;
    if (hit.n==-1 && hit.s!=-1 && hit.type==htShape) subj=tsFill;
  }
  if (subj==tsLine || subj==tsLineFill || subj==tsFill) nextlinestyle="";
  if (subj==tsLineFill || subj==tsFill) nextfillstyle="";
  if (sellimbs.size()==0 && selshapes.size()==0) return;
  MarkUndo();
  //
  if (subj==tsLine || subj==tsLineFill)
  { for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
    { TLimb &limb = body->limbs[*i];
      if (circleshape_from_limb(*i)==-1)
      { limb.color.type=ctEffect; limb.color.effect=effect;
        limb.linestyle="";
      }
    }
  }
  for (list<int>::const_iterator i=selshapes.begin(); i!=selshapes.end(); i++)
  { stk::TShape &shape = body->shapes[*i];
    if (subj==tsLine || subj==tsLineFill)
    { shape.line.type=ctEffect; shape.line.effect=effect;
      shape.linestyle="";
    }
    if (subj==tsFill || subj==tsLineFill)
    { shape.brush.type=ctEffect; shape.brush.effect=effect;
      shape.fillstyle="";
    }
  }
  ismodified=true;
  MakeEindexes(body); body->RecalcEffects(true);
  Redraw(); SetCursor();
}




void TEditor::SetBitmap(const string bn)
{ nextfillstyle="";
  if (selshapes.size()==0) return;
  MarkUndo();
  int bindex=body->bindex(bn); LUASSERT(bindex!=-1);
  for (list<int>::const_iterator i=selshapes.begin(); i!=selshapes.end(); i++)
  { stk::TShape &shape = body->shapes[*i];
    // we'll only set the bitmap if it's a circle-fill
    int li = circlelimb_from_shape(*i);
    if (li!=-1)
    { shape.brush.type=ctBitmap; shape.brush.dtype=ctBitmap;
      shape.brush.bitmap = bn;
      shape.brush.bindex = bindex;
      shape.fillstyle="";
      TLimb &limb = body->limbs[li];
      limb.color.type=ctNone; limb.color.dtype=ctNone;
    }
  }
  ismodified=true;
  if (WhitePrint) {WhitePrint=false;UpdateMenus();}
  Redraw(); SetCursor();
}

void TEditor::SetThickness(double t)
{ nextlinestyle="";
  if (sellimbs.size()==0 && selshapes.size()==0) return;
  MarkUndo();
  for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
  { TLimb &limb = body->limbs[*i];
    if (circleshape_from_limb(*i)==-1)
    { limb.thickness=t;
      if (limb.color.type==ctNone) limb.color.type=limb.color.otype;
      limb.linestyle="";
    }
  }
  for (list<int>::const_iterator i=selshapes.begin(); i!=selshapes.end(); i++)
  { stk::TShape &shape = body->shapes[*i];
    shape.thickness = t;
    if (shape.line.type==ctNone) {shape.line.type=ctRGB; shape.line.dtype=ctRGB;}
    shape.linestyle="";
  }
  ismodified=true;
  Redraw(); SetCursor();
}


void TEditor::SetOrder(int dir)
{ if (dir!=2 && dir!=-2 && dir!=1 && dir!=-1) return;
  if (sellimbs.size()==0 && selshapes.size()==0) return;
  MarkUndo();
  vector<bool> sbitmap; sbitmap.resize(body->shapes.size());
  for (list<int>::const_iterator i=selshapes.begin(); i!=selshapes.end(); i++) sbitmap[*i]=true;
  bool anyreallimbsselected=false;
  // if a circleshape+circlelimb is selected, that's not enough to count for
  // lines to have their order changed.
  for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
  { TLimb &limb = body->limbs[*i];
    if (limb.type!=3) anyreallimbsselected=true;
    else
    { int si = circleshape_from_limb(*i);
      if (si==-1) anyreallimbsselected=true;
    }
  }
  if (anyreallimbsselected)
  { for (int i=0; i<(int)body->shapes.size(); i++) {if (body->shapes[i].limbs) sbitmap[i]=true;}
  }
  // now 'sbitmaps' indicates whose order we want to change
  // we will construct a new shape-vector 'nss' with the new order
  vector<stk::TShape> nss;
  int numsel=0;
  // those of them moved to the front...
  if (dir==-2)
  { for (int i=0; i<(int)sbitmap.size(); i++)
    { if (sbitmap[i]) {numsel++; nss.push_back(body->shapes[i]);}
    }
  }
  // those of them left in the middle
  for (int i=0; i<(int)sbitmap.size(); i++)
  { if (!sbitmap[i]) nss.push_back(body->shapes[i]);
  }
  // those of them moved to the end...
  if (dir==2)
  { for (int i=0; i<(int)sbitmap.size(); i++)
    { if (sbitmap[i]) {numsel++; nss.push_back(body->shapes[i]);}
    }
  }
  body->shapes=nss;
  //
  // now let's select our changes
  selshapes.clear(); 
  if (dir==-2) {for (int i=0; i<numsel; i++) {if (!body->shapes[i].limbs) selshapes.push_back(i);}}
  else {for (int i=(int)body->shapes.size()-numsel; i<(int)body->shapes.size(); i++) {if (!body->shapes[i].limbs) selshapes.push_back(i);}}
  ismodified=true;
  Redraw(); SetCursor();
}
  



void TEditor::ButtonDown(int mx,int my)
{ if (tool==tZoom) {zoomx0=mx; zoomy0=my; zoomx=mx; zoomy=my; mode=mZoomDragging; return;}
  //
  THitTest t=PointAtCoords(mx,my,false);
  int shift = GetAsyncKeyState(VK_SHIFT)<0;
  if (!shift) {sellimbs.clear(); selshapes.clear();}
  //
  int oldhit = hit.n;
  hit.s=-1; hit.n=-1; hit.type=htMiss; hit.sf=0; hit.si=0; mode=mNothing;
  if (t.s==-1 && t.n==-1)
  { zoomx0=mx; zoomy0=my; zoomx=mx; zoomy=my; mode=mSelDragging; return;
  }
  // Otherwise, it was a click on something, and how we respond depends in a complicated
  // way on our mode and on what it was.
  if (t.n!=-1) // was it a click on a limb?
  { list<int>::iterator wassel=sellimbs.begin(); while (wassel!=sellimbs.end() && *wassel!=t.n) wassel++;
    list<int>::iterator wasselshape=selshapes.end();
    int si=circleshape_from_limb(t.n);
	if (si != -1) for (wasselshape = selshapes.begin(); wasselshape != selshapes.end() && *wasselshape != si; wasselshape++) {}
    if (shift && (wassel!=sellimbs.end() || wasselshape!=selshapes.end()))
    { if (wassel!=sellimbs.end()) sellimbs.erase(wassel);
      if (wasselshape!=selshapes.end()) selshapes.erase(wassel);
      Redraw();return;
    }
    if (oldhit>=0 && t.n==body->limbs[oldhit].root && t.type==htSpot) {hit.n=oldhit; hit.type=htRoot;} // if clicked on the root of something already selected
    else {hit.type=t.type; hit.n=t.n;}
    hit.s=t.s; sellimbs.push_back(hit.n); sellimbs.sort(); sellimbs.unique();
    if (si!=-1) {selshapes.push_back(si); selshapes.sort(); selshapes.unique();}
    mode=mNodePressing; UpdateMenus(); Redraw(); return;
  }
  // otherwise it was a click on a shape
  list<int>::iterator wassel=selshapes.begin(); while (wassel!=selshapes.end() && *wassel!=t.s) wassel++;
  list<int>::iterator wassellimb=sellimbs.end();
  int li=circlelimb_from_shape(t.s);
  if (li != -1)  for (wassellimb = sellimbs.begin(); wassellimb != sellimbs.end() && *wassellimb != li; wassellimb++) {}
  if (shift && (wassel!=selshapes.end() || wassellimb!=sellimbs.end()))
  { if (wassel!=selshapes.end()) selshapes.erase(wassel);
    if (wassellimb!=sellimbs.end()) sellimbs.erase(wassellimb);
     Redraw();return;
  }
  if (tool==tCreate && t.type==htLine) mode=mShapeLinePressing;
  else if (tool==tCreate && t.type==htSpot) mode=mShapeSpotPressing;
  else if (tool==tEdit && t.type==htSpot) mode=mShapeSpotPressing;
  hit.n=-1; hit.s=t.s; hit.type=t.type; hit.sf=t.sf; hit.si=t.si;
  selshapes.push_back(hit.s); selshapes.sort(); selshapes.unique();
  if (li!=-1) {sellimbs.push_back(li); sellimbs.sort(); sellimbs.unique();}
  UpdateMenus(); Redraw(); return;
}

void TEditor::SelectShapeLineWobble(int mx,int my)
{ THitTest t = PointAtCoords(mx,my,false);
  if (t.s==hit.s && t.si==hit.si && t.sf>=hit.sf-0.5 && t.sf<=hit.sf+0.5) return;
  // we will only ever be here because we're in create mode, and dragging a line
  mode=mCreateCornerDragging; // but won't MarkUndo until later...
  cori=hit.si; cortarget.i=-1;
  CornerMove(mx,my);
}

void TEditor::SelectShapeSpotWobble(int mx,int my)
{ THitTest t = PointAtCoords(mx,my,false);
  if (t.s==hit.s && t.si==hit.si && t.type==htSpot) return;
  MarkUndo();
  mode=mCornerMoving;
  // we'll delete this point from the list. Note that all shapes implicitly have at least two points
  stk::TShape &shape = body->shapes[hit.s];
  corotarget = shape.p[hit.si].i; //bool ar=shape.p[hit.si].ar; 
  vector<TJointRef> nss;
  for (int i=0; i<hit.si; i++) nss.push_back(shape.p[i]);
  for (int i=hit.si+1; i<(int)shape.p.size(); i++) nss.push_back(shape.p[i]);
  shape.p=nss;
  cori=hit.si-1; if (cori<0) cori+=(int)shape.p.size();
  cortarget.i=-1;
  CornerMove(mx,my);
  ShowHint("Either pin this corner to a joint, or drop it 'nowhere' to remove it");
}

void TEditor::IncNextChan()
{ if (nextchan==4) return;
  if (nextchan==0 || nextchan==1)
  { nextchan++; if (nextchan==2) {nextchan=0; nextband++; if (nextband==6) nextband=0;}
  }
}

void TEditor::SelectWobble(int mx,int my)
{ THitTest t=PointAtCoords(mx,my,false);
  if (t.n==hit.n || (hit.type==htRoot && t.n==body->limbs[hit.n].root)) return;
  // if move mouse far enough, we go into rubbber-banding to create a line
  // or just move an existing node
  if (hit.type==htRoot) {hit.n=body->limbs[hit.n].root; hit.type=htSpot;}
  if (tool==tCreate && (hit.type==htPlus || hit.type==htSpot)) // drag from a spot
  { mode=mCreateDragging;
    rubber.type=0; rubber.negative=false;
    rubber.root=hit.n;
    rubber.aisoff=true; rubber.chan=nextchan; rubber.band=nextband; rubber.anchor=0;
    rubber.cum=false; rubber.crate=1.0; rubber.creflect=0;
    rubber.aoff=0; if (rubber.root==0) rubber.aoff=-pi/2; rubber.aspring=0;
    if (hit.n==0) rubber.aisoff=false;
    rubber.ang=0;
    rubber.x0=body->limbs[hit.n].x; rubber.y0=body->limbs[hit.n].y;
    rubber.ang0=rubber.aoff; if (rubber.aisoff) rubber.ang0 += body->limbs[hit.n].ang;
    rubber.color.type=ctRGB; rubber.color.dtype=ctRGB; rubber.color.rgb=RGBCOLOR(255,255,255); rubber.thickness=1;
    rubber.freqstyle=nextfreqstyle;
    TStyle style = StyleFromName(rubber.freqstyle); if (style.name!="")
    { rubber.chan=style.limb.chan; rubber.band=style.limb.band; rubber.negative=style.limb.negative;
    }
    rubber.linestyle=nextlinestyle;
    style = StyleFromName(rubber.linestyle); if (style.name!="")
    { rubber.color=style.limb.color; rubber.thickness=style.limb.thickness;
    }
    CreatingMove(mx,my);
    IncNextChan();
  }
  else if (hit.type==htAmin) // drag an aoff handle
  { if (body->limbs[hit.n].type==0 || body->limbs[hit.n].type==1) mode=mAoffDragging;
    else if (body->limbs[hit.n].type==2 || body->limbs[hit.n].type==3) mode=mLminDragging;
    if (mode==mAoffDragging) {MarkUndo(); AngleMove(mx,my);}
    else if (mode==mLminDragging) {MarkUndo(); SpringMove(mx,my);}
  }
  else if (hit.type==htAmax) // drag an ascale handle
  { if (body->limbs[hit.n].type==0 || body->limbs[hit.n].type==1) mode=mAscaleDragging;
    else if (body->limbs[hit.n].type==2 || body->limbs[hit.n].type==3) mode=mLmaxDragging;
    if (mode==mAscaleDragging) {MarkUndo(); AngleMove(mx,my);}
    else if (mode==mLmaxDragging) {MarkUndo(); SpringMove(mx,my);}
  }
  else if (hit.type==htLine) // drag a line
  { MarkUndo();
    mode=mNodeMoving;
    RepositioningMove(hit.n,mx,my);
  }
  else if (tool==tEdit && (hit.type==htSpot || hit.type==htPlus)) // drag an existing spot
  { MarkUndo();
    mode=mNodeMoving;
    RepositioningMove(hit.n,mx,my);
  }
}

bool TEditor::SnapToGrid(int *px,int *py)
{ if (!UserGrid) return false;
  bool shift = (GetAsyncKeyState(VK_SHIFT)<0);
  if (shift) return false;
  // this code is copied out from the draw method, since both use the grid.
  double interval=1.0; int si;
  for (;;)
  { si = b2x(interval)-b2x(0); if (si<60) break; interval /= 5;
    si = b2x(interval)-b2x(0); if (si<60) break; interval /= 2;
  }
  if (si<4) return false;
  int lx=b2x(-5), ly=b2y(-5);
  int x=*px, y=*py;
  while (lx<x-4) lx+=si;
  while (ly<y-4) ly+=si;
  if (lx<=x+4 && ly<=y+4) {*px=lx; *py=ly; return true;}
  return false;
}

double TEditor::SnapAngle(double ang,bool clampzero)
{ const double c=0.2;
  if (ang<-2*pi) ang=-2*pi;
  if (ang>2*pi) ang=2*pi;
  bool shiftdown = (GetAsyncKeyState(VK_SHIFT)<0);
  if (UserSnap && shiftdown) return ang;
  if (!UserSnap && !shiftdown) return ang;
  if (clampzero) {if (ang>-c && ang<c) ang=0;}
  //
  if (ang>pi/6.0-c && ang<pi/6.0+c) ang=pi/6.0;
  if (ang>0.5*pi-c && ang<0.5*pi+c) ang=0.5*pi;
  if (ang>pi-c && ang<pi+c) ang=pi;
  if (ang>1.5*pi-c && ang<1.5*pi+c) ang=1.5*pi;
  if (ang>pi*2.0/3.0-c && ang<pi*2.0/3.0+c) ang=pi*2.0/3.0;
  if (ang>pi*4.0/3.0-c && ang<pi*4.0/3.0+c) ang=pi*4.0/3.0;
  //
  if (ang>-pi/6.0-c && ang<-pi/6.0+c) ang=-pi/6.0;
  if (ang>-0.5*pi-c && ang<-0.5*pi+c) ang=-0.5*pi;
  if (ang>-pi-c && ang<-pi+c) ang=-pi;
  if (ang>-1.5*pi-c && ang<-1.5*pi+c) ang=-1.5*pi;
  if (ang>-pi*2.0/3.0-c && ang<-pi*2.0/3.0+c) ang=-pi*2.0/3.0;
  if (ang>-pi*4.0/3.0-c && ang<-pi*4.0/3.0+c) ang=-pi*4.0/3.0;
  
  return ang;
}


void TEditor::CornerMove(int mx,int my)
{ corx=x2b(mx); cory=y2b(my);
  cortarget.i=-1;
  // but we will try to snap to the nearest target
  int dmin=-1; TJointRef jmin;
  for (int i=0; i<body->nlimbs; i++)
  { TLimb &limb = body->limbs[i];
    int x=b2x(limb.x), y=b2y(limb.y);
    int dx=mx-x, dy=my-y, dd=(dx*dx)+(dy*dy);
    if (dmin==-1 || dd<dmin) {dmin=dd; jmin.i=i; jmin.ar=false;}
    if (limb.type==1 || limb.type==3)
    { double xx,yy; jointpos(&xx,&yy,limb,true);
      x=b2x(xx); y=b2y(yy);
      dx=mx-x; dy=my-y; dd=(dx*dx)+(dy*dy);
      if (dd<dmin) {dmin=dd; jmin.i=i; jmin.ar=true;}
    }
  }
  if (dmin<13*13) // 13 pixels snap radius
  { cortarget=jmin;
    jointpos(&corx,&cory,body->limbs[cortarget.i],cortarget.ar);
  }
  Redraw();
}

void TEditor::CreatingMove(int mx,int my)
{ THitTest hit = PointAtCoords(mx,my,false);
  if (UserGrid) SnapToGrid(&mx,&my);
  rubber.x=x2b(mx); rubber.y=y2b(my);
  double dx=rubber.x-rubber.x0, dy=rubber.y-rubber.y0;
  TAdjAngle a=AngleNearbyFromOff(dx,dy,rubber.ang0,rubber.ang);
  rubber.length=a.relrad;
  rubber.lmin=rubber.length*0.4;
  rubber.ang=a.screenang;
  rubber.ascale=(rubber.ang-rubber.ang0)*1.5;
  rubber.ascale=SnapAngle(rubber.ascale,false);
  rubber.f=(rubber.ang-rubber.ang0)/rubber.ascale;
  rubber.color.type=ctRGB; rubber.color.dtype=ctRGB; rubber.color.rgb=RGBCOLOR(255,255,255); rubber.thickness=1;
  TStyle style = StyleFromName(rubber.linestyle); if (style.name!="")
  { rubber.color=style.limb.color; rubber.thickness=style.limb.thickness;
  }
  rubend.i=-1; rubber.type=0;
  if (hit.n!=-1 && hit.type==htSpot && hit.n!=rubber.root) {rubend.i=hit.n; rubend.ar=false;}
  if (hit.n!=-1 && hit.type==htAroot) {rubend.i=hit.n; rubend.ar=true;} // no worry about rubber.root, since htAroot will be the arc not the root-joint
  if (rubend.i!=-1) {rubber.thickness=2; rubber.color.type=ctRGB; rubber.color.dtype=ctRGB; rubber.color.rgb=RGBCOLOR(255,128,0);}
  body->RecalcAngles(&rubber);
  Redraw();
}

void TEditor::AngleMove(int mx,int my)
{ TLimb &limb=body->limbs[hit.n];
  if (mode==mAscaleDragging)
  { TAdjAngle a=AngleNearbyFromOff(mx-b2x(limb.x0),my-b2y(limb.y0), limb.ang0,limb.ang1);
    limb.ascale = SnapAngle(a.relang,false);
    ShowHint(StringAng(limb.ascale));
  }
  else if (mode==mAoffDragging)
  { TAdjAngle a=AngleNearbyFromOff(mx-b2x(limb.x0),my-b2y(limb.y0), limb.ang0,limb.aoff);
    double ang=limb.aoff+a.relang;
    if (ang>100 || ang<-100) ang=0; // !!! somehow it was too big
    while (ang>pi) ang-=2*pi; while (ang<-pi) ang+=2*pi;
    limb.aoff = SnapAngle(ang,true);
    ShowHint(StringAng(limb.aoff));
  }
  Recalc(hit.n);
  Redraw();
}

void TEditor::SpringMove(int mx,int my)
{ TLimb &limb=body->limbs[hit.n];
  // given the line (x0,y0)->ang, we find the point on it that's closest to (xm,ym).
  double xm=x2b(mx), ym=y2b(my);
  double x0=limb.x0, y0=limb.y0;
  double len;
  if (cos(limb.ang)<0.01 && cos(limb.ang)>-0.01)
  { if (sin(limb.ang)<0) len=(y0-ym);
    else len=(ym-y0);
  }
  else
  { double m = sin(limb.ang) / cos(limb.ang);
    double x = (m*ym + xm - m*y0 + m*m*x0) / (m*m+1);
    //double y = m*x + y0-m*x0;
    len = (x-x0) / cos(limb.ang);
  }
  if (mode==mLminDragging)
  { if (len>limb.length) len=limb.length;
    if (len<0) len=0;
    limb.lmin=len;
    ShowHint(StringLength(limb.lmin)+", "+StringFrac(limb.lmin/limb.length));
  }
  else if (mode==mLmaxDragging)
  { if (len<limb.lmin) len=limb.lmin;
    limb.length=len;
    ShowHint(StringLength(limb.length));
  }
  Recalc(hit.n);
  Redraw();
}


void TEditor::RubberMove(int mx,int my)
{ zoomx=mx; zoomy=my;
  Redraw();
}

void TEditor::SetScrollBars()
{ if (!isready) return;
  // The client area is width*height. Let's translate this into body coords.
  // note: we have "funky edge effect" using width-1 but just height.
  // This was needed for some reason, since otherwise the calling Scroll()
  // after SetScrollBars() wasn't stable (i.e. each iteration would move the image left).
  // I don't understand why - must be something to do with rounding errors or edge effects?
  s_bleft=x2b(0); s_bright=x2b(width-1); s_btop=y2b(0); s_bbottom=y2b(height);
  // the scroll bar should show body range -6 .. +6
  s_fleft=(s_bleft+6)/12; s_fright=(s_bright+6)/12; if (s_fleft<0) s_fleft=0; if (s_fright>1) s_fright=1;
  s_ftop=(s_btop+6)/12; s_fbottom=(s_bbottom+6)/12; if (s_ftop<0) s_ftop=0; if (s_fbottom>1) s_fbottom=1;
  // so now show these ranges
  SCROLLINFO si; ZeroMemory(&si,sizeof(si)); si.cbSize = sizeof(si);
  si.fMask = SIF_DISABLENOSCROLL|SIF_PAGE|SIF_RANGE|SIF_POS;
  si.nMin=0; si.nMax=10000;
  si.nPage= (int)((s_fright-s_fleft)*10000);
  si.nPos= (int)(s_fleft*10000);
  if (s_fleft==0 && s_fright==1) {si.nMin=0; si.nMax=0; si.nPage=0; si.nPos=0;}
  SetScrollInfo(chwnd,SB_HORZ,&si,TRUE);
  si.nMin=0; si.nMax=10000;
  si.nPage= (int)((s_fbottom-s_ftop)*10000);
  si.nPos= (int)(s_ftop*10000);
  if (s_ftop==0 && s_fbottom==1) {si.nMin=0; si.nMax=0; si.nPage=0; si.nPos=0;}
  SetScrollInfo(chwnd,SB_VERT,&si,TRUE);
}

void TEditor::Scroll(bool horiz,int code,int pos)
{ double nbleft=s_bleft,nbright=s_bright,nbtop=s_btop,nbbottom=s_bbottom;
  double bwidth=s_bright-s_bleft, bheight=s_bbottom-s_btop;
  bool set=true;
  if (horiz && code==SB_LINELEFT) {nbleft-=bwidth*0.1; nbright-=bwidth*0.1;}
  else if (horiz && code==SB_PAGELEFT) {nbleft-=bwidth*0.9; nbright-=bwidth*0.9;}
  else if (horiz && code==SB_LINERIGHT) {nbleft+=bwidth*0.1; nbright+=bwidth*0.1;}
  else if (horiz && code==SB_PAGERIGHT) {nbleft+=bwidth*0.9; nbright+=bwidth*0.9;}
  else if (!horiz && code==SB_LINEUP) {nbtop-=bheight*0.1; nbbottom-=bheight*0.1;}
  else if (!horiz && code==SB_PAGEUP) {nbtop-=bheight*0.9; nbbottom-=bheight*0.9;}
  else if (!horiz && code==SB_LINEDOWN) {nbtop+=bheight*0.1; nbbottom+=bheight*0.1;}
  else if (!horiz && code==SB_PAGEDOWN) {nbtop+=bheight*0.9; nbbottom+=bheight*0.9;}
  else if (horiz && (code==SB_THUMBTRACK || code==SB_THUMBPOSITION))
  { set=(code==SB_THUMBPOSITION); double dpos = ((double)pos)/10000.0;
    nbleft = -6 + dpos*12;
    nbright = nbleft + (s_bright-s_bleft);
  }
  else if (!horiz && (code==SB_THUMBTRACK || code==SB_THUMBPOSITION))
  { set=(code==SB_THUMBPOSITION); double dpos = ((double)pos)/10000.0;
    nbtop = -6 + dpos*12;
    nbbottom = nbtop + (s_bbottom-s_btop);
  }
  //
  offx=(nbleft+nbright)*-0.5; offy=(nbtop+nbbottom)*-0.5;
  Redraw();
  if (set) SetScrollBars();  

}

typedef struct {int parent; list<int> children;} TTreeNode;
typedef list<int> TPath;

int follow_path(vector<TTreeNode> &tree, int limb,int id)
{ int i=0; list<int>::const_iterator ci=tree[limb].children.begin();
  while (i<id && ci!=tree[limb].children.end()) {i++; ci++;} 
  // if we couldn't find the specified child...
  if (ci==tree[limb].children.end()) return limb;
  return *ci;
}

int follow_path(vector<TTreeNode> &tree,int limb,const list<int> &path)
{ for (list<int>::const_iterator i=path.begin(); i!=path.end(); i++)
  { int id=*i;
    limb = follow_path(tree,limb,id);
  }
  return limb;
}


void TEditor::Cursor(int dir)
{ int ilimbs=-1;
  for (int i=0; i<(int)body->shapes.size(); i++)
  { if (body->shapes[i].limbs) ilimbs=i;
  }
  list<int> ss = sellimbs;
  // if a shape had been selected, we'd still like the cursor keys to do something sensible...
  if (sellimbs.size()==0)
  { if (selshapes.size()!=0)
    { stk::TShape &shape = body->shapes[selshapes.front()];
      ss.clear();
      for (int i=0; i<(int)shape.p.size(); i++)
      { ss.push_back(shape.p[i].i);
      }
      ss.sort(); ss.unique(); 
    }
  }
  if (ss.size()==0 && (dir==0 || dir==2 || dir==1)) return;
  // 
  // now we'll get tree information about everything
  vector<TTreeNode> tree; tree.resize(body->nlimbs);
  for (int i=0; i<body->nlimbs; i++)
  { int parent = body->limbs[i].root;
    tree[i].parent = parent;
    if (i!=0) tree[parent].children.push_back(i);
  }
  // and express the selected things as indexed paths.
  list<TPath> paths; 
  for (list<int>::const_iterator i=ss.begin(); i!=ss.end(); i++)
  { TPath pa; int limb=*i;
    while (limb!=0)
    { int parent = body->limbs[limb].root;
      int sub=-1, j=0; for (list<int>::const_iterator ji=tree[parent].children.begin(); ji!=tree[parent].children.end(); ji++,j++) {if (*ji==limb) sub=j;}
      LUASSERT(sub!=-1); pa.push_front(sub); limb=parent;
    }
    paths.push_back(pa);
  }
  TPath hitpath;
  if (hit.n!=-1)
  { int limb=hit.n; while (limb!=0)
    { int parent = body->limbs[limb].root;
      int sub=-1, j=0; for (list<int>::const_iterator ji=tree[parent].children.begin(); ji!=tree[parent].children.end(); ji++,j++) {if (*ji==limb) sub=j;}
      LUASSERT(sub!=-1); hitpath.push_front(sub); limb=parent;
    }
  }
  //
  // now we have what we need!
  int sel=-1;
  if (dir==0 || dir==2) // left/right navigation
  { if (paths.size()==0) return;
    else if (paths.size()==1 || hit.n!=-1) // cycle within the current subtree
    { TPath p2; if (hit.n!=-1) p2=hitpath; else p2=paths.front();
      int last = p2.back(); p2.pop_back();
      sel = follow_path(tree,0,p2);
      int nchildren = (int)tree[sel].children.size();
      if (dir==0) last=(last+1)%nchildren; else last=(last+nchildren-1)%nchildren;
      sel=follow_path(tree,sel,last);
    }
    else // for multisel, when hit left/right, pick the leftmost/rightmost one
    { sel=0;
      while (paths.size()>0)
      { int max=-2; list<TPath>::iterator i=paths.begin();
        while (i!=paths.end())
        { int ci=i->front(); i->pop_front();
          if (dir==0) {if (max==-2) max=ci; else if (ci>max) max=ci;}
          else {if (max==-2) max=ci; else if (ci<max) max=ci;}
          if (i->size()==0) i=paths.erase(i); else i++;
          sel=follow_path(tree,sel,max);
        }
      }
    }
  }
  else if (dir==1) // for navigation up
  { if (paths.size()==0) return;
    else if (paths.size()==1 || hit.n!=-1) // for onesel, pick its parent
    { TPath p2; if (hit.n!=-1) p2=hitpath; else p2=paths.front();
      if (p2.size()<=1) return;
      p2.pop_back();
      sel = follow_path(tree,0,p2);
    }
    else // for multisel, pick the topiest
    { const TPath *p=0; int len=-2;
      for (list<TPath>::const_iterator i=paths.begin(); i!=paths.end(); i++)
      { if (len==-2) {p = &(*i); len=p->size();}
        else {if ((int)i->size()<len) {p=&(*i);len=p->size();}}
      }
      sel = follow_path(tree,0,*p);
    }
  }
  else // dir=3, for navigation down
  { if (paths.size()==0) // for nosel, pick the first child of root
    { sel = follow_path(tree,0,0);
    }
    else if (hit.n!=-1)
    { sel = follow_path(tree,0,hitpath);
      sel = follow_path(tree,sel,0);
      if (sel==-1) return;
    }
    else if (paths.size()==1) // for onesel, pick its first child
    { sel = follow_path(tree,0,paths.front());
      sel = follow_path(tree,sel,0);
      if (sel==-1) return;
    }
    else // for multisel, pick the deepest
    { const TPath *p=0; int len=-2;
      for (list<TPath>::const_iterator i=paths.begin(); i!=paths.end(); i++)
      { if (len==-2) {p=&(*i); len=p->size();}
        else {if ((int)i->size()<len) {p=&(*i);len=p->size();}}
      }
      sel = follow_path(tree,0,*p);
    }
  }
  //
  bool shift = GetAsyncKeyState(VK_SHIFT)<0;
  if (!shift)
  { sellimbs.clear(); selshapes.clear(); hit.n=-1; hit.s=-1; hit.type=htMiss;
    if (sel!=-1) {sellimbs.push_back(sel); hit.n=sel; hit.s=ilimbs;}
  }
  else
  { if (sel==-1) return;
    sellimbs.push_back(sel); sellimbs.sort(); sellimbs.unique();
    hit.n=sel; hit.s=ilimbs;
  }
  Redraw();
}
      



void TEditor::FinishSeling()
{ mode=mNothing;
  double x0=zoomx0, y0=zoomy0, x1=zoomx, y1=zoomy;
  if (zoomx<zoomx0) {x0=zoomx; x1=zoomx0;}
  if (zoomy<zoomy0) {y0=zoomy; y1=zoomy0;}
  x0=x2b((int)x0); y0=y2b((int)y0); x1=x2b((int)x1); y1=y2b((int)y1);
  // so now x0,y0 - x1,y1 is our selecting rectangle. We toggle the
  // selectidity of stuff inside it. They have to be wholly inside it.
  vector<bool> lbitmap, sbitmap; lbitmap.resize(body->nlimbs); sbitmap.resize(body->shapes.size());
  for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++) lbitmap[*i]=true;
  for (list<int>::const_iterator i=selshapes.begin();i!=selshapes.end();i++) sbitmap[*i]=true;
  //
  list<int> nl, ns;
  ObjectsInRect(x0,y0,x1,y1,nl,ns);
  //
  for (list<int>::const_iterator i=nl.begin(); i!=nl.end(); i++) lbitmap[*i] = !lbitmap[*i];
  for (list<int>::const_iterator i=ns.begin(); i!=ns.end(); i++) sbitmap[*i] = !sbitmap[*i];
  sellimbs.clear(); selshapes.clear();
  for (int i=0; i<body->nlimbs; i++) {if (lbitmap[i]) sellimbs.push_back(i);}
  for (int i=0; i<(int)body->shapes.size(); i++) {if (sbitmap[i]) selshapes.push_back(i);}
  hit.n=-1; hit.s=-1; hit.type=htMiss;
  Redraw();
  UpdateMenus();
}


void TEditor::Zoom(int dir)
{ if (dir==1) zoom*=2;
  else if (dir==-1) zoom*=0.5;
  else
  { offx=0; offy=0; 
    double w=width, h=height;
    zoom = w/12; if (h/12<zoom) zoom=h/12;
  }
  if (zoom<1) zoom=1;
  if (zoom>1000000) zoom=1000000;
  Redraw(); SetCursor();
  SetScrollBars();
}

void TEditor::FinishZooming()
{ mode=mNothing;
  double x0=zoomx0, y0=zoomy0, x1=zoomx, y1=zoomy;
  if (zoomx<zoomx0) {x0=zoomx; x1=zoomx0;}
  if (zoomy<zoomy0) {y0=zoomy; y1=zoomy0;}
  double dx=x1-x0, dy=y1-y0;
  double dd=sqrt(dx*dx+dy*dy);
  if (dd<10)
  { zoom=zoom*0.5; if (zoom<1) zoom=1;
    Recalc(0); Redraw(); SetCursor(); SetScrollBars(); ShowHint("ZOOM mode. (Right-click to change mode)"); return;
  }
  //
  x0=x2b((int)x0); y0=y2b((int)y0); x1=x2b((int)x1); y1=y2b((int)y1);
  double w=width, h=height;
  offx=(x0+x1)*-0.5; offy=(y0+y1)*-0.5;
  double zoom1=w/(x1-x0), zoom2=h/(y1-y0);
  zoom=zoom1; if (zoom2<zoom) zoom=zoom2;
  if (zoom>1000000) zoom=1000000;
  Recalc(0);
  Redraw(); SetCursor();
  SetScrollBars();  
  //
  ShowHint("ZOOM mode. (Right-click to change mode)");
}

void TEditor::FinishAngling()
{ mode=mNothing;
  ismodified=true;
  ShowHint("");
}

void TEditor::FinishSpringing()
{ mode=mNothing;
  ismodified=true;
  ShowHint("");
}

  
  

void TEditor::RepositioningMove(int n,int mx,int my,bool suppresscalcdraw)
{ TLimb &limb=body->limbs[n];
  bool gsnap=false; if (UserGrid) gsnap=SnapToGrid(&mx,&my);
  double x=x2b(mx), y=y2b(my);
  TAdjAngle a=AngleFromOff(x-limb.x0,y-limb.y0,limb.ang0,limb.aoff,limb.ascale);
  if ((limb.type==0 || limb.type==1) && limb.chan==4 && limb.band==0) // fixed. rootang+f.
  { a=AngleNearbyFromOff(x-limb.x0,y-limb.y0,limb.ang0,limb.ascale);
    double ang=a.screenang; ang-=limb.aoff; if (limb.aisoff) ang-=body->limbs[limb.root].ang;
    // We'll now fix it up so it's close to the old value. In fact, AngleNearbyFromOff was
    // supposed to do that, but it's a bit hard to debug...
    // ang is the angle. But it could be (say) 5.0, which could be interpreted as +5 or as -1.28.
    // so we will pick whichever one is closer to ascale.
    while (ang<0) ang+=2*pi; while (ang>2*pi) ang-=2*pi;
    double ang_other = ang-2*pi;
    double d=ang-limb.ascale, d_other=ang_other-limb.ascale;
    d=d*d; d_other=d_other*d_other;
    if (d_other<d) ang=ang_other;
    //
    limb.ascale=ang*1.1; if (limb.ascale<-2*pi) limb.ascale=-2*pi;
    if (limb.ascale>2*pi) limb.ascale=2*pi; // so that ascale indicates the direction of the arc
    while (ang<-2*pi) ang+=2*pi; while (ang>2*pi) ang-=2*pi;
    limb.f=ang; if (n!=0 && !gsnap) limb.f=SnapAngle(limb.f,true);
    limb.length=a.relrad; limb.lmin=limb.length*0.4;
    ShowHint(StringAng(limb.f)+", "+StringLength(a.relrad));
  }
  else if (limb.type==0 || limb.type==1) // normal. dragging f within an ascale
  { if (!a.isin) return;
    limb.f=a.f; limb.length=a.relrad; limb.lmin=limb.length*0.4;
    ShowHint(StringFrac(limb.f)+", "+StringLength(a.relrad));
    SetF(n,limb.chan,limb.band,limb.f,suppresscalcdraw);
    return;
  }
  else if (limb.type==2 || limb.type==3) // a spring. dragging the point, and so adjusting angle and f
  { double ang=a.screenang; if (limb.aisoff) ang-=body->limbs[limb.root].ang;
    while (ang<0) ang+=2*pi; while (ang>2*pi) ang-=2*pi;
    if (n!=0 && !gsnap) ang=SnapAngle(ang,true);
    limb.aspring=ang-limb.aoff;
    double lf=a.relrad;
    if (limb.chan==4 && limb.band==0) // a fixed: the limits are of no concern to us
    { //if (lf>limb.length) limb.length=lf*1.1;
      if (lf<limb.lmin) limb.lmin=lf*0.9;
      limb.length=lf; limb.f=1;
      ShowHint(StringAng(ang)+", "+StringLength(lf));
    }
    else
    { if (lf>limb.length) lf=limb.length; if (lf<limb.lmin) lf=limb.lmin;
      limb.f = (lf-limb.lmin) / (limb.length-limb.lmin);
      limb.f = 1-limb.f;
      ShowHint(StringAng(ang)+", "+StringFrac(limb.f));
    }
  }
  ismodified=true;
  if (!suppresscalcdraw) {Recalc(n); Redraw(); SetCursor();}
}

void TEditor::SetF(int n, int chan,int band, double f,bool suppresscalcdraw)
{ if (chan==4) return;
  vector<bool> ancestors; ancestors.resize(body->nlimbs);
  while (n!=0) {ancestors[n]=true; n=body->limbs[n].root;}
  for (int i=0; i<body->nlimbs; i++)
  { int achan=body->limbs[i].chan, aband=body->limbs[i].band;
    if (achan==chan && aband==band && !ancestors[i])
    { body->limbs[i].f=f;
      if (body->limbs[i].negative) body->limbs[i].f = 1.0-body->limbs[i].f;
    }
  }
  if (!suppresscalcdraw) {Recalc(0); Redraw(); SetCursor();}
}




void TEditor::Unselect()
{ mode=mNothing;
  if (hit.type!=htRoot) {Redraw();SetCursor();return;} 
  // If user clicked on the selected root, that changes its aoff mode
  // but we want to preserve the effective angle
  int root=body->limbs[hit.n].root;
  if (root==0) {Redraw(); SetCursor();return;}
  double rang=body->limbs[root].ang;
  //double ang=body->limbs[hit.n].aoff;
  if (body->limbs[hit.n].aisoff)
  { body->limbs[hit.n].aisoff=false;
    body->limbs[hit.n].aoff += rang;
  }
  else
  { body->limbs[hit.n].aisoff=true;
    body->limbs[hit.n].aoff -= rang;
  }
  ismodified=true;
  Redraw();
  SetCursor();
  ShowHint("");
}


void TEditor::FinishCreateCorner()
{ stk::TShape &shape = body->shapes[hit.s];
  if (cortarget.i==-1)
  { if (mode==mCreateCornerDragging) {cori=-1; mode=mNothing;  Redraw(); return;}
    // upon move, failure is only allowed if the thing will be left with at least two different points
    TJointRef pt = shape.p[0];
    for (int i=1; i<(int)shape.p.size(); i++)
    { if (shape.p[i]!=pt) {cori=-1; mode=mNothing; Redraw(); return;}
    }
    cortarget=corotarget; // otherwise, we just create back the point we removed when we started dragging
  }
  if (mode==mCreateCornerDragging) MarkUndo();
  mode=mNothing;
  // we will update the shape with a new point, unless it's a duplicate of its neighbour
  ismodified=true;
  bool isdup=false;
  if (shape.p[cori]==cortarget) isdup=true;
  if (shape.p[(cori+1)%shape.p.size()]==cortarget) isdup=true;
  vector<TJointRef> np; np.reserve(shape.p.size()+1);
  for (int i=0; i<cori+1; i++) np.push_back(shape.p[i]);
  if (!isdup) np.push_back(cortarget);
  for (int i=cori+1; i<(int)shape.p.size(); i++) np.push_back(shape.p[i]);
  shape.p = np;
  hit.si=cori; hit.type=htLine;
  cori=-1;
  Redraw();  
  UpdateMenus();
}

void TEditor::FinishCreate()
{ MarkUndo();
  sellimbs.clear(); selshapes.clear(); hit.n=-1; hit.s=-1; hit.type=htMiss;
  if (rubend.i==-1) // did we just create a normal limb?
  { int n=body->CreateLimb(rubber); int mx=b2x(rubber.x), my=b2y(rubber.y);
    Recalc(n);
    if (rubber.chan==4 && rubber.band==0) RepositioningMove(n,mx,my,false);
    // that's a hack. Before, it was putting the newly created fixed limb in the wrong place
    hit.n=n; hit.type=htSpot; // select the thing we've just created
    sellimbs.push_back(n);
  }
  else // or we created a shape (i.e. a line)
  { stk::TShape shape; shape.brush.type=ctRGB; shape.brush.dtype=ctRGB; shape.brush.rgb=RGBCOLOR(defcols[cYellow].r,defcols[cYellow].g,defcols[cYellow].b);
    shape.line.type=ctRGB; shape.line.dtype=ctRGB; shape.line.rgb=RGBCOLOR(128,128,128); shape.thickness=1;
    shape.limbs=false; shape.balternate=false;
    TJointRef j(rubber.root,false); shape.p.push_back(j);
    shape.p.push_back(rubend);
    shape.linestyle=nextlinestyle;
    TStyle style=StyleFromName(shape.linestyle); if (style.name!="")
    { shape.line=style.limb.color; shape.thickness=style.limb.thickness;
    }
    shape.fillstyle=nextfillstyle;
    style=StyleFromName(shape.fillstyle); if (style.name!="")
    { shape.brush=style.shape.brush;
      if (shape.brush.type==ctBitmap)
      { for (int i=0; i<(int)body->bmps.size(); i++)
        { if (StringLower(shape.brush.bitmap)==StringLower(body->bmps[i].name)) shape.brush.bindex=i;
        }
      }
    }
    body->shapes.push_back(shape);
    // and now reset our rubber
    rubend=-1;
    // and select the thing we've just created
    hit.n=-1; hit.s=(int)body->shapes.size()-1; hit.sf=0; hit.si=0; hit.type=htLine;
    selshapes.push_back(hit.s);
  }
  mode=mNothing;
  ismodified=true;
  UpdateMenus();
  Redraw();
  ShowHint("CREATE mode. (Right-click to go back to EDIT mode)");
}

void TEditor::FinishRepositioning()
{ mode=mNothing;
  ismodified=true;
  Redraw();
  ShowHint("");
} 

void TEditor::Size(int awidth,int aheight)
{ width=awidth; height=aheight;
  int cx=GetSystemMetrics(SM_CXSCREEN), cy=GetSystemMetrics(SM_CYSCREEN);
  BITMAP bmp; GetObject(memBM,sizeof(bmp),&bmp);
  if (bmp.bmWidth<width || bmp.bmHeight<height)
  { int width2=min(width*3/2,cx), height2=min(height*3/2,cy);
    SelectObject(memDC,oldBM);
    DeleteObject(memBM);
    HDC hdc=GetDC(chwnd);
    memBM = CreateCompatibleBitmap(hdc,width2,height2);
    ReleaseDC(chwnd,hdc);
    oldBM = (HBITMAP)SelectObject(memDC,memBM);
  }
  Recalc(0);
  SetScrollBars();
}


void TEditor::Invert()
{ WhitePrint = !WhitePrint;
  RegSaveUser();
  Redraw();
  UpdateMenus();
}

void TEditor::Animate()
{ mode=mNothingTesting;
  timeStart=GetTickCount(); timePrev=timeStart;
  samplei = rand()%3;
  unsigned int maxtime = (samplesize[samplei]/12)*41;
  timeStart += rand()%maxtime;
  idTimer=SetTimer(mhwnd,1,100,NULL);
}

void TEditor::StopAnimate()
{ if (idTimer!=0) KillTimer(mhwnd,idTimer); idTimer=0;
  mode=mNothing;
  Tick(true);
}



void TEditor::Tick(bool)
{ unsigned int now=GetTickCount();
  unsigned int diff=now-timePrev; if (diff<(unsigned int)(1000/body->fps)) return;
  // our notional standard is 10ms between frames. We use this to scale the cumulative stuff
  double cmul=((double)diff)/10.0;
  //
  timePrev=now;
  unsigned int time=now-timeStart;
  unsigned int off = (time/41)%samplesize[samplei];
  unsigned char *sd = sampledat[samplei]+off*12;
  double freq[3][6];
  for (int chan=0; chan<2; chan++)
  { for (int band=0; band<6; band++)
    { unsigned char s = sd[chan*6+band];
      double d = ((double)s)/255.0;
      freq[chan][band]=d;
    }
    freq[2][0]=freq[0][3]; freq[2][1]=freq[1][4];
  }
  // do the limb positions
  body->AssignFreq(freq,cmul); 
  body->Recalc(); body->RecalcEffects(true);
  Redraw();
}

void TEditor::DrawPiePart(HDC hdc,int x0,int y0,double length,double ang0,double ang,double ang1)
{ int ilength=(int)(length+0.5);
  double angA,angB,angC,angD; // fill in A->B, line C->D. All will be anticlockwise.
  if (ang1<=ang0) {angA=ang0; angB=ang; angC=ang; angD=ang1;}
  else {angA=ang; angB=ang0; angC=ang1; angD=ang;}
  int dxA=x0+(int)(cos(angA)*length), dyA=y0+(int)(sin(angA)*length);
  int dxB=x0+(int)(cos(angB)*length), dyB=y0+(int)(sin(angB)*length);
  int dxC=x0+(int)(cos(angC)*length), dyC=y0+(int)(sin(angC)*length);
  int dxD=x0+(int)(cos(angD)*length), dyD=y0+(int)(sin(angD)*length);
  int ddAB=(dxB-dxA)*(dxB-dxA)+(dyB-dyA)*(dyB-dyA);
  int ddCD=(dxD-dxC)*(dxD-dxC)+(dyD-dyC)*(dyD-dyC);
  if (ddAB<100 && angA-angB<pi) {POINT pt[]={{x0,y0},{dxA,dyA},{dxB,dyB}}; Polygon(hdc,pt,3);}
  else Pie(hdc,x0-ilength,y0-ilength,x0+ilength,y0+ilength, dxA,dyA, dxB,dyB);
  if (ddCD<100 && angC-angD<pi) {MoveToEx(hdc,dxC,dyC,NULL); LineTo(hdc,dxD,dyD);}
  else Arc(hdc,x0-ilength,y0-ilength,x0+ilength,y0+ilength, dxC,dyC, dxD,dyD);
}

void TEditor::DrawLinePart(HDC hdc,double thick,double ang,int x0,int y0,int x1,int y1,int x2,int y2)
{ double ddx=cos(ang+0.5*pi)*thick, ddy=sin(ang+0.5*pi)*thick;
  int dx=(int)ddx, dy=(int)ddy;
  POINT pt[5];
  pt[0].x=x0+dx; pt[0].y=y0+dy;
  pt[1].x=x1+dx; pt[1].y=y1+dy;
  pt[2].x=x1-dx; pt[2].y=y1-dy;
  pt[3].x=x0-dx; pt[3].y=y0-dy;
  Polygon(hdc,pt,4);
  pt[0].x=x2+dx; pt[0].y=y2+dy;
  pt[3].x=x2-dx; pt[3].y=y2-dy;
  pt[4]=pt[0];
  Polyline(hdc,pt,5);
}
 


void TEditor::DrawPie(HDC hdc,TLimb &dst,int sp,bool fillall)
{ if (dst.chan==4) return;
  COLORREF col=RGB(0,0,0),col2=RGB(0,0,0); int c1=dst.band*15+20, c2=dst.band*20+130;
  if (dst.chan==0) col=RGB(c1,c1,c2);
  else if (dst.chan==1) col=RGB(c2,c1,c1);
  else if (dst.chan==2) {col=RGB(c1,c1,c2); col2=RGB(c2,c1,c1);}
  else if (dst.chan==3) col=RGB(c1,c2,c1);
  else col=RGB(128,128,128);
  HPEN hPiePen=CreatePen(PS_SOLID,0,col);
  LOGBRUSH lb; lb.lbColor=col; lb.lbStyle=BS_SOLID; HBRUSH hPieBrush=CreateBrushIndirect(&lb);
  HPEN hPiePen2=NULL; HBRUSH hPieBrush2=NULL;
  HPEN holdp=(HPEN)SelectObject(hdc,hPiePen);
  HBRUSH holdb=(HBRUSH)SelectObject(hdc,hPieBrush);
  double dsp=sp;
  //
  // A straightforward angle limb. Draw filled:aoff->ang, empty:ang->ascale. fillall means fill it all
  if ((dst.type==0 || dst.type==1) && (dst.chan==0 || dst.chan==1 || dst.chan==3))
  { if (!dst.negative) DrawPiePart(hdc,b2x(dst.x0),b2y(dst.y0),dsp,dst.ang0,fillall?dst.ang1:dst.ang,dst.ang1);
    else DrawPiePart(hdc,b2x(dst.x0),b2y(dst.y0),dsp,dst.ang1,fillall?dst.ang0:dst.ang,dst.ang0);
  }
  //
  // A straightforward spring limb. Draw filled:length->f, empty:f->lmin.
  if ((dst.type==2 || dst.type==3) && (dst.chan==0 || dst.chan==1 || dst.chan==3))
  { int x[3],y[3]; double lf=dst.lmin*dst.f+dst.length*(1-dst.f);
    x[0]=b2x(dst.x0+dst.lmin*cos(dst.ang)); y[0]=b2y(dst.y0+dst.lmin*sin(dst.ang));
    x[1]=b2x(dst.x0+lf*cos(dst.ang)); y[1]=b2y(dst.y0+lf*sin(dst.ang));
    x[2]=b2x(dst.x0+dst.length*cos(dst.ang)); y[2]=b2y(dst.y0+dst.length*sin(dst.ang));
    if (!dst.negative) DrawLinePart(hdc,dsp/2,dst.ang,x[2],y[2],fillall?x[0]:x[1],fillall?y[0]:y[1],x[0],y[0]);
    else DrawLinePart(hdc,dsp/2,dst.ang,x[0],y[0],fillall?x[2]:x[1],fillall?y[2]:y[1],x[2],y[2]);
  }
  //
  // A difference angle limb.
  // filled:middle->ang (in either col1 or col2); empty:ang->aoff, middle->ascale
  // fillall means filled1:middle->aoff, filled2:middle->ascale
  if ((dst.type==0 || dst.type==1) && dst.chan==2)
  { double angMin,angLeft,angMid,angRight,angMax; // min->left and max->right are hollow; mid->left and mid->right are full
    angMid=(dst.ang0+dst.ang1)/2; angMin=dst.ang0; angMax=dst.ang1;
    if (dst.f>=0.5) {angLeft=angMid; angRight=dst.ang;}
    else {angLeft=dst.ang; angRight=angMid;}
    if (fillall) {angLeft=angMin; angRight=angMax;}
    //
    hPiePen2=CreatePen(PS_SOLID,0,col2);
    lb.lbColor=col2; hPieBrush2=CreateBrushIndirect(&lb);
    if (!dst.negative) DrawPiePart(hdc,b2x(dst.x0),b2y(dst.y0),dsp,angMid,angLeft,angMin);
    else DrawPiePart(hdc,b2x(dst.x0),b2y(dst.y0),dsp,angMid,angRight,angMax);
    SelectObject(hdc,hPiePen2); SelectObject(hdc,hPieBrush2);
    if (!dst.negative) DrawPiePart(hdc,b2x(dst.x0),b2y(dst.y0),dsp,angMid,angRight,angMax);
    else DrawPiePart(hdc,b2x(dst.x0),b2y(dst.y0),dsp,angMid,angLeft,angMin);
  }
  //
  // A difference spring limb. filled: middle->f (in either col1 or col2); empty: lmin->middle, f->length
  // filledall means filled1:middle->lmin, filled2:middle->lmax
  // Oops! actually, I reversed the sense of f.
  if ((dst.type==2 || dst.type==3) && dst.chan==2)
  { int x[4],y[4]; double lf=dst.lmin*dst.f+dst.length*(1-dst.f), lm=dst.lmin*0.5+dst.length*0.5;
    x[0]=b2x(dst.x0+dst.lmin*cos(dst.ang)); y[0]=b2y(dst.y0+dst.lmin*sin(dst.ang));
    x[1]=b2x(dst.x0+lf*cos(dst.ang)); y[1]=b2y(dst.y0+lf*sin(dst.ang));
    x[2]=b2x(dst.x0+lm*cos(dst.ang)); y[2]=b2y(dst.y0+lm*sin(dst.ang));
    x[3]=b2x(dst.x0+dst.length*cos(dst.ang)); y[3]=b2y(dst.y0+dst.length*sin(dst.ang));
    hPiePen2=CreatePen(PS_SOLID,0,col2);
    lb.lbColor=col2; hPieBrush2=CreateBrushIndirect(&lb);
    if (fillall)
    { if (!dst.negative) DrawLinePart(hdc,dsp/2,dst.ang,x[2],y[2],x[0],y[0],x[0],y[0]);
      else DrawLinePart(hdc,dsp/2,dst.ang,x[2],y[2],x[3],y[3],x[3],y[3]);
      SelectObject(hdc,hPiePen2); SelectObject(hdc,hPieBrush2);
      if (!dst.negative) DrawLinePart(hdc,dsp/2,dst.ang,x[2],y[2],x[3],y[3],x[3],y[3]);
      else DrawLinePart(hdc,dsp/2,dst.ang,x[2],y[2],x[0],y[0],x[0],y[0]);
    }
    else if (dst.f>=0.5)
    { if (!dst.negative) DrawLinePart(hdc,dsp/2,dst.ang,x[2],y[2],x[1],y[1],x[0],y[0]);
      else DrawLinePart(hdc,dsp/2,dst.ang,x[2],y[2],x[2],y[2],x[3],y[3]);
      SelectObject(hdc,hPiePen2); SelectObject(hdc,hPieBrush2);
      if (!dst.negative) DrawLinePart(hdc,dsp/2,dst.ang,x[2],y[2],x[2],y[2],x[3],y[3]);
      else DrawLinePart(hdc,dsp/2,dst.ang,x[2],y[2],x[1],y[1],x[0],y[0]);
    }
    else
    { if (!dst.negative) DrawLinePart(hdc,dsp/2,dst.ang,x[2],y[2],x[2],y[2],x[0],y[0]);
      else DrawLinePart(hdc,dsp/2,dst.ang,x[2],y[2],x[1],y[1],x[3],y[3]);
      SelectObject(hdc,hPiePen2); SelectObject(hdc,hPieBrush2);
      if (!dst.negative) DrawLinePart(hdc,dsp/2,dst.ang,x[2],y[2],x[1],y[1],x[3],y[3]);
      else DrawLinePart(hdc,dsp/2,dst.ang,x[2],y[2],x[2],y[2],x[0],y[0]);
    }
  }
  //
  SelectObject(hdc,holdb);
  SelectObject(hdc,holdp);
  DeleteObject(hPiePen);
  DeleteObject(hPieBrush);
  if (hPiePen2!=NULL) DeleteObject(hPiePen2);
  if (hPieBrush2!=NULL) DeleteObject(hPieBrush2);
}



class TAutoPen
{ public:
  HPEN hpen,hprev;
  bool set(double thickness,bool sel,bool vis,int r,int g,int b)
  { if (r==-1) {r=avert?0:255; g=avert?0:255; b=avert?0:255;}
    if (r==255 && g==255 && b==255) {r=avert?0:255; g=avert?0:255; b=avert?0:255;}
    if (thickness==athick && sel==asel && vis==avis && r==ar && g==ag && b==ab && hpen!=0) return false;
    if (hpen!=0)
    { if (hprev!=0) DeleteObject(hprev);
      hprev=hpen;
    }
    int style = vis?PS_SOLID:PS_DOT;
    int width=0;
    if (vis)
    { double tt=thickness;
      tt = tt+1.0;
      tt = tt-1.0;
      if (tt<0.5) width=0;
      else if (tt<1.5) width=1;
      else width=2;
      if (!avert) width*=2;
    }
    if (sel&&vis) {width+=avert?1:2;}
    if (style==PS_DOT) width=1;
    if (!vis && !ShowInvisibles) style=PS_NULL;
    hpen = CreatePen(style, width, RGB(r,g,b));
    athick=thickness; asel=sel; avis=vis; ar=r; ag=g; ab=b;
    return true;
  }
  TAutoPen(bool vert) {hpen=0; hprev=0; avert=vert;}
  ~TAutoPen() {if (hpen!=0) DeleteObject(hpen); hpen=0; if (hprev!=0) DeleteObject(hprev); hprev=0;}
protected:
  double athick; bool asel,avis; int ar,ag,ab; bool avert;
};




bool TEditor::Draw(HDC hwndc, string *err)
{ bool fail=false;
  //
  HDC hdc=memDC;
  SetGraphicsMode(hdc,GM_ADVANCED);
  SetStretchBltMode(hdc,COLORONCOLOR);
  HDC tdc=CreateCompatibleDC(hwndc); HGDIOBJ holdt=0; // for any bitmaps we might draw
  //
  bool vert=WhitePrint;
  if (body->limbs[0].color.rgb.r>240 && body->limbs[0].color.rgb.g>240 && body->limbs[0].color.rgb.b>240) vert=true;
  HPEN hAnglePen=CreatePen(PS_SOLID,0,RGB(64,192,222));
  HPEN hPlusPen=CreatePen(PS_SOLID,0,RGB(212,32,32));
  TAutoPen lpen(vert);
  HPEN hRubberPen=CreatePen(PS_DOT,0,RGB(vert?0:240,vert?0:240,vert?0:240));
  HPEN hAnchorPen=CreatePen(PS_SOLID,0,RGB(182,182,36));
  HPEN hBorderPen=CreatePen(PS_DOT,0,RGB(182,182,182));
  LOGBRUSH lb; lb.lbColor=RGB(212,32,32); lb.lbStyle=BS_SOLID; HBRUSH hAngleBrush=CreateBrushIndirect(&lb);
  HBRUSH holdb=(HBRUSH)SelectObject(hdc,GetStockObject(BLACK_BRUSH));
  HPEN holdp=(HPEN)SelectObject(hdc,GetStockObject(BLACK_PEN));
  //
  RECT rc={0,0,width,height};
  lb.lbColor = RGB(body->limbs[0].color.rgb.r, body->limbs[0].color.rgb.g, body->limbs[0].color.rgb.b);
  HBRUSH hbackbrush = CreateBrushIndirect(&lb);
  if (WhitePrint) SetBkColor(hdc,RGB(255,255,255)); else SetBkColor(hdc,lb.lbColor);
  if (WhitePrint) FillRect(hdc,&rc,(HBRUSH)GetStockObject(WHITE_BRUSH));
  else FillRect(hdc,&rc,hbackbrush);
  DeleteObject(hbackbrush);
  Scale(-5,-5,(int*)&rc.left,(int*)&rc.top); Scale(5,5,(int*)&rc.right,(int*)&rc.bottom);
  if (hbackdrop!=0)
  { HDC htempdc=CreateCompatibleDC(hdc); HGDIOBJ hold=SelectObject(htempdc,hbackdrop);
    BITMAP bmp; GetObject(hbackdrop,sizeof(bmp),&bmp);
    StretchBlt(hdc,rc.left,rc.top,rc.right-rc.left,rc.bottom-rc.top,htempdc,0,0,bmp.bmWidth,bmp.bmHeight,SRCCOPY);
    SelectObject(htempdc,hold); DeleteObject(htempdc);
  }
  SelectObject(hdc,hBorderPen); SelectObject(hdc,GetStockObject(NULL_BRUSH));
  Rectangle(hdc,rc.left,rc.top,rc.right,rc.bottom);
  if (UserGrid)
  { // we'll find the interval (1,0.1,...) which gives a nice spacing
    double interval=1.0; int si;
    for (;;)
    { si = b2x(interval)-b2x(0); if (si<60) break;
      interval /= 5;
      si = b2x(interval)-b2x(0); if (si<60) break;
      interval /= 2;
    }
    if (si>4)
    { int lx=b2x(-5), ly=b2y(-5), rx=b2x(5), ry=b2y(5);
      while (lx<0) lx+=si; while (rx>width) rx-=si;
      while (ly<0) ly+=si; while (ry>height) ry-=si;
      for (int y=ly; y<ry; y+=si)
      { for (int x=lx; x<rx; x+=si)
        { SetPixel(hdc,x,y,RGB(182,182,182));
        }
      }
    }
  }
  int n;
  //
  // First draw any anchors
  SelectObject(hdc,hAnchorPen);
  for (n=1; n<body->nlimbs; n++)
  { TLimb &limb=body->limbs[n];
    int x=b2x(limb.x), y=b2y(limb.y); int aw=AnchorLength/2, ao=AnchorOffLength;
    if (limb.anchor==1) {MoveToEx(hdc,x-aw,y-ao,NULL); LineTo(hdc,x+aw,y-ao);} // north
    else if (limb.anchor==2) {MoveToEx(hdc,x+ao,y-aw,NULL); LineTo(hdc,x+ao,y+aw);} // east
    else if (limb.anchor==3) {MoveToEx(hdc,x-aw,y+ao,NULL); LineTo(hdc,x+aw,y+ao);} // south
    else if (limb.anchor==4) {MoveToEx(hdc,x-ao,y-aw,NULL); LineTo(hdc,x-ao,y+aw);} // west
  }
  //
  // Now maybe draw the current line's editing pie
  list<TLimb*> sels;
  if (mode==mCreateDragging) {if (rubend.i==-1) sels.push_back(&rubber);}
  else
  { for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++) sels.push_back(&body->limbs[*i]);
  }
  for (list<TLimb*>::const_iterator si=sels.begin(); si!=sels.end(); si++) DrawPie(hdc,**si,SelPieLength,false);
  //
  // Draw angle pies on every limb
  for (n=1; n<body->nlimbs && ShowAngles; n++) DrawPie(hdc,body->limbs[n],PieLength,true);
  //
  // Draw the shapes and lines. In mode==mCreateDragging we also draw the rubber.
  //
  TEffPt ept;
  for (int sn=0; sn<(int)body->shapes.size(); sn++)
  { const stk::TShape &shape = body->shapes[sn];
    if (!shape.limbs)
    { SetPolyFillMode(hdc,shape.balternate?ALTERNATE:WINDING);
      bool heavy=false;
      for (list<int>::const_iterator si=selshapes.begin(); si!=selshapes.end(); si++) {if (sn==*si) heavy=true;}
      bool simplecircle = (shape.p.size()==2 && shape.p[0].i==shape.p[1].i && shape.p[0].ar!=shape.p[1].ar && body->limbs[shape.p[0].i].type==3);
      if (shape.brush.dtype==ctBitmap && (shape.brush.bindex<0 || shape.brush.bindex>=(int)body->bmps.size())) {fail=true; if (err!=0) *err="bindex";}
      if (simplecircle && shape.brush.dtype==ctBitmap && shape.brush.bindex!=-1)
      { TLimb &limb = body->limbs[shape.p[0].i];
        double dx=limb.x-limb.x0,dy=limb.y-limb.y0, rad=sqrt(dx*dx+dy*dy);
        int x0=b2x(limb.x0-rad), y0=b2y(limb.y0-rad), x1=b2x(limb.x0+rad), y1=b2y(limb.y0+rad);
        TBmp &bmp = body->bmps[shape.brush.bindex];
        // adjust the rect so the bitmap is kept proportional
        double f=((double)bmp.bwidth)/((double)bmp.bheight);
        if (f>1) {int cy=(y0+y1)/2, h=y1-y0; h=(int)(((double)h)/f); y0=cy-h/2; y1=cy+h/2;}
        else if (f<1) {int cx=(x0+x1)/2, w=x1-x0; w=(int)(((double)w)*f); x0=cx-w/2; x1=cx+w/2;}
        XFORM mat; FLOAT ang=(FLOAT)(limb.ang);
        FLOAT c=(FLOAT)cos(ang), s=(FLOAT)sin(ang);
        FLOAT cx=(FLOAT)(x0+(x1-x0)/2), cy=(FLOAT)(y0+(y1-y0)/2);
        mat.eM11=c; mat.eM12=s; mat.eM21=-s; mat.eM22=c;
        mat.eDx=cx+cy*s-cx*c;  mat.eDy=cy-cy*c-cx*s;
        // If we're too zoomed in, then we won't draw the bitmap.
        bool toobig = (x1-x0>width*2 || y1-y0>height*2);
        FLOAT xtl=(FLOAT)x0, xbl=(FLOAT)x0, ytl=(FLOAT)y0, ytr=(FLOAT)y0, xtr=(FLOAT)x1, xbr=(FLOAT)x1, ybl=(FLOAT)y1, ybr=(FLOAT)y1;
        FLOAT xtl2=xtl*mat.eM11+ytl*mat.eM21+mat.eDx, ytl2=xtl*mat.eM12+ytl*mat.eM22+mat.eDy;
        FLOAT xtr2=xtr*mat.eM11+ytr*mat.eM21+mat.eDx, ytr2=xtr*mat.eM12+ytr*mat.eM22+mat.eDy;
        FLOAT xbl2=xbl*mat.eM11+ybl*mat.eM21+mat.eDx, ybl2=xbl*mat.eM12+ybl*mat.eM22+mat.eDy;
        FLOAT xbr2=xbr*mat.eM11+ybr*mat.eM21+mat.eDx, ybr2=xbr*mat.eM12+ybr*mat.eM22+mat.eDy;
        LONG ixtl=(LONG)xtl2, ixtr=(LONG)xtr2, ixbl=(LONG)xbl2, ixbr=(LONG)xbr2;
        LONG iytl=(LONG)ytl2, iytr=(LONG)ytr2, iybl=(LONG)ybl2, iybr=(LONG)ybr2;
        bool offthescreen = (ixtr<0 && ixbr<0) || (ixtl>width && ixbl>width);
        offthescreen |= (iybl<0 && iybr<0) || (iytl>height && iytr>height);
        if (!offthescreen && (toobig || (WhitePrint && bmp.hbmask==0)))
        { POINT pt[5]; pt[0].x=ixtl; pt[0].y=iytl; pt[1].x=ixtr; pt[1].y=iytr;
          pt[2].x=ixbr; pt[2].y=iybr; pt[3].x=ixbl; pt[3].y=iybl;
          SelectObject(hdc,GetStockObject(LTGRAY_BRUSH));
          SelectObject(hdc,GetStockObject(NULL_PEN));
          Polygon(hdc,pt,4);
        }
        else if (!offthescreen)
        { if (ang!=0) SetWorldTransform(hdc,&mat);
          if (bmp.hbmask!=0)
          { if (holdt==0) holdt=SelectObject(tdc,bmp.hbmask); else SelectObject(tdc,bmp.hbmask);
            if (WhitePrint)
            { SelectObject(hdc,GetStockObject(LTGRAY_BRUSH));
              StretchBlt(hdc,x0,y0,x1-x0,y1-y0,tdc,0,0,bmp.bwidth,bmp.bheight,0x00A803A9);
              // that's a ternary op: (brush OR mask) AND hdc -- because mask uses 255 for,
              // the mask area, and 0 for the solid, we OR it to get 128 for the solid area.
            }
            else StretchBlt(hdc,x0,y0,x1-x0,y1-y0,tdc,0,0,bmp.bwidth,bmp.bheight,SRCAND);
          }
          if (bmp.hbm!=0 && !WhitePrint)
          { if (holdt==0) holdt=SelectObject(tdc,bmp.hbm); else SelectObject(tdc,bmp.hbm);
            if (bmp.hbmask!=0) StretchBlt(hdc,x0,y0,x1-x0,y1-y0,tdc,0,0,bmp.bwidth,bmp.bheight,SRCPAINT);
            else StretchBlt(hdc,x0,y0,x1-x0,y1-y0,tdc,0,0,bmp.bwidth,bmp.bheight,SRCCOPY);
          }
          mat.eM11=1; mat.eM12=0; mat.eM21=0; mat.eM22=1; mat.eDx=0; mat.eDy=0;
          if (ang!=0) SetWorldTransform(hdc,&mat);
        }
      }
      if (simplecircle)
      { TLimb &limb = body->limbs[shape.p[0].i];
        double dx=limb.x-limb.x0,dy=limb.y-limb.y0, rad=sqrt(dx*dx+dy*dy);
        int x0=b2x(limb.x0-rad), y0=b2y(limb.y0-rad), x1=b2x(limb.x0+rad), y1=b2y(limb.y0+rad);
        HPEN hspen=CreatePen(PS_NULL,0,0); HGDIOBJ hop=SelectObject(hdc,hspen);
        if (shape.line.dtype==ctNone) {}
        else if (shape.line.dtype==ctRGB)
        { lpen.set(shape.thickness,heavy,true,shape.line.rgb.r,shape.line.rgb.g,shape.line.rgb.b);
          SelectObject(hdc,lpen.hpen);
        }
        else {fail=true; if (err!=0) *err="shape line colour";}
        LOGBRUSH lbr; 
        if (shape.brush.dtype==ctNone || shape.brush.dtype==ctBitmap) lbr.lbStyle=BS_NULL;
        else if (shape.brush.dtype==ctRGB) {lbr.lbStyle=BS_SOLID; lbr.lbColor=RGB(shape.brush.rgb.r,shape.brush.rgb.g,shape.brush.rgb.b);}
        else if (shape.brush.dtype==ctDefault) {lbr.lbStyle=BS_SOLID; lbr.lbColor=vert?RGB(0,0,0):RGB(255,255,255);}
        else {fail=true; if (err!=0) *err="shape brush colour";}
        HBRUSH hsbr=CreateBrushIndirect(&lbr); HGDIOBJ hob = SelectObject(hdc,hsbr);
        Ellipse(hdc,x0,y0,x1,y1);
        SelectObject(hdc,hob); DeleteObject(hsbr);
        SelectObject(hdc,hop); DeleteObject(hspen);
      }
      else
      { // not a simple circle, so we do it the long way...,
        // first, build the pt[] array.
        int ptpos=0; for (int si=0; si<(int)shape.p.size(); si++) 
        { TJointRef j0=shape.p[si], j1=shape.p[(si+1)%shape.p.size()];
          TLimb &limb0=body->limbs[j0.i]; //, &limb1=body->limbs[j1.i];
          if (j0.i==j1.i && j0.ar!=j1.ar && (limb0.type==1 || limb0.type==3)) ptpos=add_arc(gb2x,gb2y,this,&ept,ptpos,limb0,j0.ar,-1);
          else ptpos=add_pt(gb2x,gb2y,this,&ept,ptpos,limb0,j0.ar,-1);
        }
        if (shape.p.size()>2) ptpos=ept.add(ptpos,ept.pt[0].x,ept.pt[0].y,shape.p.size()-1); // to close it!      
        if (shape.brush.dtype==ctRGB || shape.brush.dtype==ctDefault)
        { HPEN hspen=CreatePen(PS_NULL,0,0); HGDIOBJ hop = SelectObject(hdc,hspen);
          LOGBRUSH lbr; lbr.lbStyle=BS_SOLID;
          lbr.lbColor=RGB(shape.brush.rgb.r,shape.brush.rgb.g,shape.brush.rgb.b); if (shape.brush.dtype==ctDefault) lbr.lbColor=vert?RGB(0,0,0):RGB(255,255,255);
          HBRUSH hsbr=CreateBrushIndirect(&lbr); HGDIOBJ hob = SelectObject(hdc,hsbr);
          Polygon(hdc,ept.pt,ptpos);
          SelectObject(hdc,hob); DeleteObject(hsbr);
          SelectObject(hdc,hop); DeleteObject(hspen);
        }
        double tt = shape.thickness;
        lpen.set(tt,heavy,shape.line.dtype==ctRGB,shape.line.rgb.r,shape.line.rgb.g,shape.line.rgb.b);
        SelectObject(hdc,lpen.hpen);
        Polyline(hdc,ept.pt,ptpos);
      }
    }
    // now draw the actual lines
    if (shape.limbs)
    { int tot=body->nlimbs; if (mode==mCreateDragging) tot++;
      for (n=1; n<tot; n++)
      { TLimb *limb; if (n<body->nlimbs) limb=&body->limbs[n]; else limb=&rubber;
        bool heavy=false;
        if (n==body->nlimbs) heavy=true; // for the pretend line that is our create-drag
        if (mode!=mCreateDragging)
        { for (list<int>::const_iterator si=sellimbs.begin(); si!=sellimbs.end(); si++) {if (n==*si) heavy=true;}
        }
        bool changed = lpen.set(limb->thickness,heavy,limb->color.dtype==ctRGB,limb->color.rgb.r,limb->color.rgb.g,limb->color.rgb.b);
        if (changed||n==1) SelectObject(hdc,lpen.hpen);
        bool isarc = (limb->type==1 || limb->type==3 || (limb==&rubber && rubber.root==rubend.i && rubend.ar));
        if (!isarc) // a line or spring
        { MoveToEx(hdc,b2x(limb->x0),b2y(limb->y0),NULL); LineTo(hdc,b2x(limb->x),b2y(limb->y));
        }
        else // an arc. 
        { bool isrubarc = (limb==&rubber && rubber.root==rubend.i && rubend.ar);
          // rubarc:  origin at rubber->root->x0y0, radius=rubber->root->length, ends at rubber->root->ang0,1
          // normarc: origin at limb->x0y0, radius=limb->length, ends=limb->ang0,1
          TLimb *arclimb=limb;
          if (isrubarc) arclimb=&body->limbs[limb->root];
          double d=arclimb->length;
          double angA,angB;
          if (arclimb->type==3) {angA=arclimb->ang; angB=angA-1.96*pi; double dx=arclimb->x-arclimb->x0,dy=arclimb->y-arclimb->y0; d=sqrt(dx*dx+dy*dy);}
          else if (arclimb->ascale<0) {angA=arclimb->ang0;angB=arclimb->ang;}
          else {angA=arclimb->ang; angB=arclimb->ang0;}
          int xA=b2x(arclimb->x0+d*cos(angA)), yA=b2y(arclimb->y0+d*sin(angA));
          int xB=b2x(arclimb->x0+d*cos(angB)), yB=b2y(arclimb->y0+d*sin(angB));
          int ddAB=(xB-xA)*(xB-xA)+(yB-yA)*(yB-yA);
          bool suppresscircle=false;
          if (limb->type==3 && limb!=&rubber && limb->color.dtype!=ctNone)
          { int ci=circleshape_from_limb(n); if (ci!=-1 && body->shapes[ci].line.dtype!=ctNone) suppresscircle=true;
          }
          if (!suppresscircle)
          { if (ddAB<100 && angA-angB<pi) {MoveToEx(hdc,xA,yA,NULL); LineTo(hdc,xB,yB);}
            else Arc(hdc,b2x(arclimb->x0-d),b2y(arclimb->y0-d),b2x(arclimb->x0+d),b2y(arclimb->y0+d), xA,yA, xB,yB);
          }
        }
      }
    }
  }
  //
  // Draw all the joints. We count how this joint is used: 0=all offset, 1=mixed, 2=all fixed
  vector<int> numoff,numfix,use;
  numoff.resize(body->nlimbs); numfix.resize(body->nlimbs); use.resize(body->nlimbs);
  for (n=0; n<body->nlimbs; n++) {numoff[n]=0; numfix[n]=0;}
  for (n=0; n<body->nlimbs; n++) {int r=body->limbs[n].root; if (body->limbs[n].aisoff) numoff[r]++; else numfix[r]++;}
  for (n=0; n<body->nlimbs; n++) {if (numfix[n]==0) use[n]=0; else if (numoff[n]==0) use[n]=2; else use[n]=1;}
  SelectObject(hdc,GetStockObject(vert?WHITE_PEN:BLACK_PEN));
  SelectObject(hdc,GetStockObject(vert?BLACK_BRUSH:WHITE_BRUSH));
  if (ShowJoints)
  { for (n=0; n<body->nlimbs; n++)
    { TLimb &limb=body->limbs[n]; int x0=b2x(limb.x), y0=b2y(limb.y);
      if (use[n]<2)  Ellipse(hdc,x0-SpotLength,y0-SpotLength,x0+SpotLength,y0+SpotLength);
      if (use[n]==1) Rectangle(hdc,x0-SpotLength,y0-SpotLength,x0,y0+SpotLength);
      if (use[n]==2) Rectangle(hdc,x0-SpotLength,y0-SpotLength,x0+SpotLength,y0+SpotLength);
    }
  }
  else
  { for (list<int>::const_iterator i=sellimbs.begin(); i!=sellimbs.end(); i++)
    { TLimb &limb=body->limbs[*i]; int x0=b2x(limb.x), y0=b2y(limb.y);
      if (use[n]<2)  Ellipse(hdc,x0-SpotLength,y0-SpotLength,x0+SpotLength,y0+SpotLength);
      if (use[n]==1) Rectangle(hdc,x0-SpotLength,y0-SpotLength,x0,y0+SpotLength);
      if (use[n]==2) Rectangle(hdc,x0-SpotLength,y0-SpotLength,x0+SpotLength,y0+SpotLength);
    }
  }
  //
  // corner-creating nodules for while we're in the middle of dragging a vertex
  if (cori!=-1)
  { stk::TShape &shape = body->shapes[hit.s];
    // we now draw the orange lines from i0 to corx,cory to i2.
    // if corx,cory are actually a cortarget, then the orange lines might conceivably be points
    int i0=cori, i2=(cori+1)%shape.p.size();
    TJointRef j0=shape.p[i0], j1=cortarget, j2=shape.p[i2];
    int ptpos=0;
    if (j0.i==cortarget.i && j0.ar!=cortarget.ar) ptpos=add_arc(gb2x,gb2y,this,&ept,ptpos,body->limbs[j0.i],j0.ar,-1);
    else ptpos=add_pt(gb2x,gb2y,this,&ept,ptpos,body->limbs[j0.i],j0.ar,-1);
    if (j2.i==cortarget.i && j2.ar!=cortarget.ar) ptpos=add_arc(gb2x,gb2y,this,&ept,ptpos,body->limbs[j2.i],!j2.ar,-1);
    else ptpos=add_pt(gb2x,gb2y,this,&ept,ptpos,corx,cory,-1);
    ptpos=add_pt(gb2x,gb2y,this,&ept,ptpos,body->limbs[j2.i],j2.ar,-1);
    lpen.set(2,false,true,255,128,0); SelectObject(hdc,lpen.hpen);
    Polyline(hdc,ept.pt,ptpos);
  }
  // shape handles on the currently selected shapes
  for (list<int>::const_iterator si=selshapes.begin(); si!=selshapes.end(); si++)
  { stk::TShape &shape = body->shapes[*si];
    bool simplecircle = (shape.p.size()==2 && shape.p[0].i==shape.p[1].i && body->limbs[shape.p[0].i].type==3);
    SelectObject(hdc,hAnglePen);
    SelectObject(hdc,GetStockObject(BLACK_BRUSH));
    int avx=0, avy=0, avn=(int)shape.p.size();
    for (int li=0; li<(int)shape.p.size(); li++)
    { TJointRef j = shape.p[li];
      TLimb &limb = body->limbs[j.i]; bool ar=j.ar;
      double xx,yy; jointpos(&xx,&yy,limb,ar);
      int x=b2x(xx), y=b2y(yy);
      if (!simplecircle) Rectangle(hdc,x-ArmSpotLength,y-ArmSpotLength,x+ArmSpotLength,y+ArmSpotLength);
      avx+=x; avy+=y;
    }
    avx/=avn; avy/=avn;
    { // a circle
      TLimb &limb = body->limbs[shape.p[0].i];
      avx = b2x(0.3*limb.x0 + 0.7*limb.x);
      avy = b2y(0.3*limb.y0 + 0.7*limb.y);
    }
    Rectangle(hdc,avx-ArmSpotLength,avy-ArmSpotLength,avx+ArmSpotLength,avy+ArmSpotLength);
  }
  // handles for selection
  for (list<TLimb*>::const_iterator si=sels.begin(); si!=sels.end(); si++)
  { TLimb *sel=*si;
    // angle handles
    if (sel->type==0 || sel->type==1)
    { SelectObject(hdc,hAnglePen);
      SelectObject(hdc,hAngleBrush);
      double d=ArmLength;
      int x0=b2x(sel->x0)+(int)(d*cos(sel->ang0)), y0=b2y(sel->y0)+(int)(d*sin(sel->ang0));
      int x1=b2x(sel->x0)+(int)(d*cos(sel->ang1)), y1=b2y(sel->y0)+(int)(d*sin(sel->ang1));
      if (sel->chan==4 && sel->band==1) {fail=true; if (err!=0) *err="I thought I got rid of freq 4.1";}
      bool showmin=(sel->chan!=4 || sel->band!=0), showmax=showmin;
      if (sel->type==1) showmin=true;      
      if (showmin)
      { SelectObject(hdc,GetStockObject(BLACK_BRUSH));
        MoveToEx(hdc,b2x(sel->x0),b2y(sel->y0),NULL); LineTo(hdc,x0,y0); Rectangle(hdc,x0-ArmSpotLength,y0-ArmSpotLength,x0+ArmSpotLength,y0+ArmSpotLength);
      }
      if (showmax)
      { SelectObject(hdc,GetStockObject(GRAY_BRUSH));
        MoveToEx(hdc,b2x(sel->x0),b2y(sel->y0),NULL); LineTo(hdc,x1,y1); Rectangle(hdc,x1-ArmSpotLength,y1-ArmSpotLength,x1+ArmSpotLength,y1+ArmSpotLength);
      }
    }
    // spring handles
    if (sel->type==2 || sel->type==3)
    { SelectObject(hdc,hAnglePen);
      double d=ArmLength;
      double ddx=cos(sel->ang+0.5*pi)*d/2, ddy=sin(sel->ang+0.5*pi)*d/2;
      int dx=(int)ddx, dy=(int)ddy;
      //double lf=sel->lmin*(1-sel->f)+sel->length*sel->f;
      int x,y, x0,y0,x1,y1;
      if (sel->chan==4 && sel->band==1) {fail=true; if (err!=0) *err="I thought I got rid of freq 4.1";}
      bool showmin=(sel->chan!=4 || sel->band!=0), showmax=showmin;
      if (showmax)
      { x=b2x(sel->x0+sel->lmin*cos(sel->ang)); y=b2y(sel->y0+sel->lmin*sin(sel->ang));
        x0=x+dx; y0=y+dy; x1=x-dx; y1=y-dy;
        SelectObject(hdc,GetStockObject(GRAY_BRUSH));
        MoveToEx(hdc,x0,y0,NULL); LineTo(hdc,x1,y1); Rectangle(hdc,x1-ArmSpotLength,y1-ArmSpotLength,x1+ArmSpotLength,y1+ArmSpotLength);
      }
      if (showmin)
      { x=b2x(sel->x0+sel->length*cos(sel->ang)); y=b2y(sel->y0+sel->length*sin(sel->ang));
        x0=x+dx; y0=y+dy; x1=x-dx; y1=y-dy;
        SelectObject(hdc,GetStockObject(BLACK_BRUSH));
        MoveToEx(hdc,x0,y0,NULL); LineTo(hdc,x1,y1); Rectangle(hdc,x1-ArmSpotLength,y1-ArmSpotLength,x1+ArmSpotLength,y1+ArmSpotLength);
      }
    }
  }
  // rubber-band zooming
  if (mode==mZoomDragging || mode==mSelDragging)
  { SelectObject(hdc,GetStockObject(NULL_BRUSH));
    SelectObject(hdc,hRubberPen);
    Rectangle(hdc,zoomx0,zoomy0,zoomx,zoomy);
  }

  //
  SelectObject(hdc,holdb);
  SelectObject(hdc,holdp);
  DeleteObject(hAnglePen);
  DeleteObject(hPlusPen);
  DeleteObject(hAngleBrush);
  DeleteObject(hRubberPen);
  DeleteObject(hAnchorPen);
  DeleteObject(hBorderPen);
  if (holdt!=0) SelectObject(tdc,holdt); DeleteDC(tdc);
  //
  BitBlt(hwndc,0,0,width,height,hdc,0,0,SRCCOPY);
  //
  return !fail;
}

  



int WINAPI AboutDlgProc(HWND hdlg,UINT msg,WPARAM,LPARAM)
{ switch (msg)
  { case WM_INITDIALOG:
    { // 1. the VersionInfo resource
      string vi="???";
      DWORD hVersion; char fn[MAX_PATH]; GetModuleFileName(hInstance,fn,MAX_PATH);
      DWORD vis=GetFileVersionInfoSize(fn,&hVersion);
      if (vis!=0)
      { char *vData = new char[(UINT)vis];
        if (GetFileVersionInfo(fn,hVersion,vis,vData))
        { char vn[100];
          strcpy(vn,"\\VarFileInfo\\Translation");
          LPVOID transblock; UINT vsize;
          BOOL res=VerQueryValue(vData,vn,&transblock,&vsize);
          if (res)
          { DWORD ot = *(DWORD*)transblock, lo=(HIWORD(ot))&0xffff, hi=((LOWORD(ot))&0xffff)<<16;
            *(DWORD *)transblock = lo|hi;
            wsprintf(vn,"\\StringFileInfo\\%08lx\\%s",*(DWORD *)transblock,"FileVersion");
            char *ver;
            res=VerQueryValue(vData,vn,(LPVOID*)&ver,&vsize);
            if (res)
            { char c[100]; char *d=c;
              while (*ver!=0)
              { if (*ver==',') {*d='.'; d++; ver++;}
                else if (*ver==' ') {*ver++;}
                else {*d=*ver; d++; ver++;}
              }
              *d=0; vi=string(c);
            }
          }
        }
        delete[] vData;
      }
      // 2. the body's version number
      string bv="???";
      TBody *b = new TBody();
      bv = b->version;
      delete b;
      // 3. the version written in the manifest
      string mv="???";
      HRSRC hrsrc = FindResource(hInstance,MAKEINTRESOURCE(1),MAKEINTRESOURCE(24)); // 24 is RT_MANIFEST
      if (hrsrc!=NULL)
      { HANDLE hglob = LoadResource(hInstance,hrsrc);
        DWORD size = SizeofResource(hInstance,hrsrc);
        if (hglob!=NULL)
        { char *dat = (char*)LockResource(hglob);
          char *nd = new char[size]; memcpy(nd,dat,size); nd[size-1]=0;
          // we assume that the first assembly mentioned in the file is the file itself
          char *assemstart = strstr(nd,"<assemblyIdentity");
          char *assemend=0; if (assemstart!=0) assemend = strstr(assemstart,"/>");
          if (assemstart!=0 && assemend!=0)
          { *assemend=0;
            char *verstart = strstr(assemstart,"version=");
            char *verend=0; if (verstart!=0) verend = strstr(verstart,"\n");
            if (verstart!=0 && verend!=0)
            { verstart=verstart+8; *verend=0;
              if (*verstart=='"') verstart++;
              verend--; if (*verend=='\r') verend--; if (*verend=='"') verend--;
              verend++;
              mv=string(verstart,verend-verstart);
            }
          }
        }
      }
      string msg = "version "+vi+" (core "+bv+" manifest "+mv+")";
      SetDlgItemText(hdlg,ID_VERSION,msg.c_str());
      //
      void SetDlgItemUrl(HWND hdlg,int id,const char *url) ;
      SetDlgItemUrl(hdlg,ID_HREF,"http://www.wischik.com/lu/senses/sticky");
      return TRUE;
    }
    case WM_COMMAND: EndDialog(hdlg,IDOK); return TRUE;
  }
  return FALSE;
}


//LRESULT CALLBACK DebugWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
//{ TEditor *editor;
//  #pragma warning( push ) // my code is clean! but the compiler won't believe me...
//  #pragma warning( disable : 4244 4312 )
//  if (msg==WM_CREATE)
//  { CREATESTRUCT *cs=(CREATESTRUCT*)lParam;
//    editor=(TEditor*)cs->lpCreateParams;
//    SetWindowLongPtr(hwnd,GWLP_USERDATA,(LONG_PTR)editor);
//  }
//  else
//  { editor=(TEditor*)GetWindowLongPtr(hwnd,GWLP_USERDATA);
//  }
//  #pragma warning( pop )
//  switch (msg)
//  { case WM_PAINT:
//    { PAINTSTRUCT ps; BeginPaint(hwnd,&ps);
//      RECT rc; GetClientRect(hwnd,&rc);
//      editor->DebugPaint(ps.hdc,rc);
//      EndPaint(hwnd,&ps);
//    } break;
//  }
//  return DefWindowProc(hwnd,msg,wParam,lParam);
//}

//HWND CreateDebugWindow(TEditor *editor)
//{ WNDCLASSEX wcex; ZeroMemory(&wcex,sizeof(wcex)); wcex.cbSize = sizeof(WNDCLASSEX); 
//  BOOL res=GetClassInfoEx(hInstance,"Stick3DebugClass",&wcex);
//  if (!res)
//  { wcex.style = CS_HREDRAW | CS_VREDRAW;
//    wcex.lpfnWndProc = (WNDPROC)DebugWndProc;
//    wcex.cbClsExtra = 0;
//    wcex.cbWndExtra = 0;
//    wcex.hInstance = hInstance;
//    wcex.hIcon = LoadIcon(NULL,MAKEINTRESOURCE(1));
//    wcex.hCursor = LoadCursor(NULL, IDC_ARROW);
//    wcex.hbrBackground = (HBRUSH)GetStockObject(BLACK_BRUSH);
//    wcex.lpszMenuName	= NULL;
//    wcex.lpszClassName = "Stick3DebugClass";
//    wcex.hIconSm = NULL;
//    ATOM res=RegisterClassEx(&wcex);
//    if (res==0) {MessageBox(NULL,"Failed to register class","Error",MB_OK); return 0;}
//  }
//  //
//  HWND hwnd = CreateWindowEx(0,"Stick3DebugClass", "Sticky Debug",
//      WS_POPUP,
//      900, 100, 124, 400, NULL, NULL, hInstance, editor);
//  if (hwnd==NULL) {MessageBox(NULL,"Failed to create window","Error",MB_OK); return 0;}
//  ShowWindow(hwnd,SW_SHOW);
//  return hwnd;
//}


LRESULT CALLBACK ClientWndProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ TEditor *editor;
  #pragma warning( push ) // my code is clean! but the compiler won't believe me...
  #pragma warning( disable : 4244 4312 )
  if (msg==WM_CREATE) {CREATESTRUCT *cs=(CREATESTRUCT*)lParam; editor=(TEditor*)cs->lpCreateParams; SetWindowLongPtr(hwnd,GWLP_USERDATA,(LONG_PTR)editor);}
  else {editor=(TEditor*)GetWindowLongPtr(hwnd,GWLP_USERDATA);}
  #pragma warning( pop )
  if (editor==NULL) return DefWindowProc(hwnd,msg,wParam,lParam);
  switch (msg)
  { case WM_PAINT:
	{ PAINTSTRUCT ps; HDC hdc=BeginPaint(hwnd, &ps);
    string err; bool ok=editor->Draw(hdc,&err);
	  EndPaint(hwnd, &ps);
    if (!ok) editor->ShowHint(err);
	  return 0;
    }
    case WM_ERASEBKGND: return 1;
    case WM_HSCROLL: editor->Scroll(true,LOWORD(wParam),HIWORD(wParam)); break;
    case WM_VSCROLL: editor->Scroll(false,LOWORD(wParam),HIWORD(wParam)); break;
    case WM_RBUTTONDOWN:
    { if (editor->mode==mNothing) editor->RightClick(LOWORD(lParam),HIWORD(lParam));
      else if (editor->mode==mNothingTesting) editor->StopAnimate();
      break;
    }
    case WM_LBUTTONDOWN:
    { if (editor->mode==mNothing) editor->ButtonDown(LOWORD(lParam),HIWORD(lParam));
      else if (editor->mode==mNothingTesting) editor->StopAnimate();
      break;
    }
    case WM_MOUSEMOVE:
    { int x=LOWORD(lParam), y=HIWORD(lParam);
      if (editor->mode==mNothing) editor->HoverMonitor(x,y);
      else if (editor->mode==mNodePressing) editor->SelectWobble(x,y);
      else if (editor->mode==mShapeLinePressing) editor->SelectShapeLineWobble(x,y);
      else if (editor->mode==mShapeSpotPressing) editor->SelectShapeSpotWobble(x,y);
      else if (editor->mode==mCreateDragging) editor->CreatingMove(x,y);
      else if (editor->mode==mCreateCornerDragging) editor->CornerMove(x,y);
      else if (editor->mode==mCornerMoving) editor->CornerMove(x,y);
      else if (editor->mode==mNodeMoving) editor->RepositioningMove(editor->hit.n,x,y);
      else if (editor->mode==mAoffDragging) editor->AngleMove(x,y);
      else if (editor->mode==mAscaleDragging) editor->AngleMove(x,y);
      else if (editor->mode==mLminDragging) editor->SpringMove(x,y);
      else if (editor->mode==mLmaxDragging) editor->SpringMove(x,y);
      else if (editor->mode==mZoomDragging) editor->RubberMove(x,y);
      else if (editor->mode==mSelDragging) editor->RubberMove(x,y);
      break;
    }
    case WM_LBUTTONUP:
    { if (editor->mode==mNodePressing) editor->Unselect();
      else if (editor->mode==mShapeLinePressing) editor->Unselect();
      else if (editor->mode==mShapeSpotPressing) editor->Unselect();
      else if (editor->mode==mCreateDragging) editor->FinishCreate();
      else if (editor->mode==mCreateCornerDragging) editor->FinishCreateCorner();
      else if (editor->mode==mCornerMoving) editor->FinishCreateCorner();
      else if (editor->mode==mNodeMoving) editor->FinishRepositioning();
      else if (editor->mode==mAoffDragging) editor->FinishAngling();
      else if (editor->mode==mAscaleDragging) editor->FinishAngling();
      else if (editor->mode==mLminDragging) editor->FinishSpringing();
      else if (editor->mode==mLmaxDragging) editor->FinishSpringing();
      else if (editor->mode==mZoomDragging) editor->FinishZooming();
      else if (editor->mode==mSelDragging) editor->FinishSeling();
      break;
    }
    case WM_SIZE:
    { editor->Size(LOWORD(lParam),HIWORD(lParam));
      break;
    }
    case WM_SETCURSOR:
    { if (LOWORD(lParam)==HTCLIENT) {editor->SetCursor(); return 0;}
      break;
    }
  }
  return DefWindowProc(hwnd,msg,wParam,lParam);  
}


LRESULT CALLBACK EditWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{ TEditor *editor;
  #pragma warning( push ) // my code is clean! but the compiler won't believe me...
  #pragma warning( disable : 4244 4312 )
  if (msg==WM_CREATE) {CREATESTRUCT *cs=(CREATESTRUCT*)lParam; editor=(TEditor*)cs->lpCreateParams; SetWindowLongPtr(hwnd,GWLP_USERDATA,(LONG_PTR)editor);}
  else {editor=(TEditor*)GetWindowLongPtr(hwnd,GWLP_USERDATA);}
  #pragma warning( pop )
  switch (msg)
  { case WM_CREATE:
    { //hdb = CreateDebugWindow(editor);
      return 0;
    }
	case WM_DESTROY:
    { PostQuitMessage(0);
	  return 0;
    }
    case WM_TIMER:
    { if (editor->mode==mNothingTesting) editor->Tick(false);
      break;
    }
    case WM_COMMAND:
    { int id=LOWORD(wParam); //code=HIWORD(wParam) 
      if (editor->mode==mNothing && id==ID_FILE_NEW) editor->FileNew();
      else if (editor->mode==mNothing && id==ID_FILE_OPEN) editor->FileOpen("");
      else if (editor->mode==mNothing && id==ID_FILE_SAVE) editor->FileSave();
      else if (editor->mode==mNothing && id==ID_FILE_SAVEAS) editor->FileSaveAs(editor->curfile);
      else if (editor->mode==mNothing && id==ID_FILE_EXPORTAVI) editor->FileExport();
      else if (editor->mode==mExporting && id==ID_FILE_EXPORTAVI) editor->ExportCancelled=true;
      else if (editor->mode==mExporting && id==ID_EXPORTCANCEL) editor->ExportCancelled=true;
      else if (id==ID_FILE_EXIT) SendMessage(hwnd,WM_CLOSE,0,0);
      else if (editor->mode==mNothing && id==ID_EDIT_INSERT && editor->mode==mNothing && editor->sellimbs.size()==1) editor->InsertLimb();
      else if (editor->mode==mNothing && id==ID_EDIT_DELETE && editor->mode==mNothing) editor->DeleteLimbOrShape();
      else if (editor->mode==mNothing && id==ID_EDIT_CUT && editor->mode==mNothing) editor->Cut();
      else if (editor->mode==mNothing && id==ID_EDIT_COPY && editor->mode==mNothing) editor->Copy();
      else if (editor->mode==mNothing && id==ID_EDIT_PASTE && editor->mode==mNothing) editor->Paste();
      else if (editor->mode==mNothing && id==ID_EDIT_FLIP) editor->Flip();
      else if (editor->mode==mNothing && id==ID_EDIT_ENLARGE) editor->Stretch(1);
      else if (editor->mode==mNothing && id==ID_EDIT_SHRINK) editor->Stretch(-1);
      else if (editor->mode==mNothing && id==ID_EDIT_UNDO) editor->EditUndo();
      else if (editor->mode==mNothing && id==ID_EDIT_REDO) editor->EditRedo();
      else if (editor->mode==mNothing && id==ID_EDIT_MARK) editor->MarkUndo();
      else if (editor->mode==mNothing && id==ID_CDOWN) editor->Cursor(3);
      else if (editor->mode==mNothing && id==ID_CLEFT) editor->Cursor(0);
      else if (editor->mode==mNothing && id==ID_CUP) editor->Cursor(1);
      else if (editor->mode==mNothing && id==ID_CRIGHT) editor->Cursor(2);
      else if ((editor->mode==mNothing || editor->mode==mNothingTesting) && id==ID_TOOLS_TEST) {if (editor->mode==mNothingTesting) editor->StopAnimate(); else editor->Animate();}
      else if (id==ID_TOOLS_SHOWANGLES) {ShowAngles=!ShowAngles; RegSaveUser(); editor->UpdateMenus(); editor->Redraw();}
      else if (id==ID_TOOLS_SHOWJOINTS) {ShowJoints=!ShowJoints; RegSaveUser(); editor->UpdateMenus(); editor->Redraw();}
      else if (id==ID_TOOLS_SHOWINVISIBLES) {ShowInvisibles=!ShowInvisibles; RegSaveUser(); editor->UpdateMenus(); editor->Redraw();}
      else if (id==ID_TOOLS_SHOW) {bool olds=(ShowAngles|ShowJoints|ShowInvisibles), news=!olds; ShowAngles=news; ShowJoints=news; ShowInvisibles=news; RegSaveUser(); editor->UpdateMenus(); editor->Redraw();}
      else if (editor->mode==mNothing && id==ID_TOOLS_ZOOMMODE) {editor->tool=tZoom; editor->ShowHint("");}
      else if (editor->mode==mNothing && id==ID_TOOLS_EDITMODE) {editor->tool=tEdit; editor->ShowHint("");}
      else if (editor->mode==mNothing && id==ID_TOOLS_CREATEMODE) {editor->tool=tCreate; editor->ShowHint("");}
      else if (editor->mode==mNothing && id==ID_TOOLS_HITTEST) {editor->ShowHitTest();}
      else if (editor->mode==mNothing && id==ID_TOOLS_USERCOLOURS) editor->UserColours(editor->mhwnd);
      else if (editor->mode==mNothing && id==ID_TOOLS_USERCUMULATIVES) editor->UserCumulatives(editor->mhwnd);
      else if (editor->mode==mNothing && id==ID_TOOLS_USERTHICKNESS) editor->UserThicknesses(editor->mhwnd);
      else if (editor->mode==mNothing && id==ID_TOOLS_BITMAPS) editor->Bitmaps(editor->mhwnd);
      else if (editor->mode==mNothing && id==ID_TOOLS_SNAP) {UserSnap=!UserSnap; RegSaveUser(); editor->UpdateMenus();}
      else if (editor->mode==mNothing && id==ID_TOOLS_GRID) {UserGrid=!UserGrid; RegSaveUser(); editor->UpdateMenus(); editor->Redraw();}
      else if (editor->mode==mNothing && id==ID_TOOLS_BACKDROP) {if (editor->hbackdrop==0) editor->Backdrop("?"); else editor->Backdrop("");}
      else if (editor->mode==mNothing && id==ID_TOOLS_CHECKUPRIGHT) editor->MaybeCheckUpright(true);
      else if (editor->mode==mNothing && id==ID_TOOLS_STYLES) editor->Styles(editor->mhwnd);
      else if (editor->mode==mNothing && id==ID_TOOLS_EFFECTS) editor->Effects(editor->mhwnd);
      else if (editor->mode==mNothing && id==ID_TOOLS_CATEGORY) editor->Category();
      else if (editor->mode==mNothing && id==ID_TOOLS_TARGETFRAMERATE) editor->TargetFramerate();
      else if (id==ID_INVERT) editor->Invert();
      else if (editor->mode==mNothing && id==ID_HELP_ABOUT) {DialogBox(hInstance,MAKEINTRESOURCE(IDD_ABOUT),hwnd,AboutDlgProc);}
      else if (editor->mode==mNothing && id==ID_HELP_HELP)
      { string hlp = GetStickDir(true)+"\\sticky.txt";
        ShellExecute(hwnd,"open",hlp.c_str(),NULL,NULL,SW_SHOWNORMAL);
      }
      else if (id==ID_ZOOMIN) editor->Zoom(1);
      else if (id==ID_ZOOMOUT) editor->Zoom(-1);
      else if (id==ID_ZOOMFULL) editor->Zoom(0);
      else if (editor->mode==mNothing && id>=ID_STYLEA && id<=ID_STYLET) editor->SetStyle((char)(id+'A'-ID_STYLEA));
      else if (editor->mode==mNothing && id==ID_FIXED) editor->SetChanBand(4,0);
      else if (editor->mode==mNothing && id==ID_LEFT) editor->SetChanBand(0,-1);
      else if (editor->mode==mNothing && id==ID_RIGHT) editor->SetChanBand(1,-1);
      else if (editor->mode==mNothing && id==ID_DIFFERENCE) editor->SetChanBand(2,-1);
      else if (editor->mode==mNothing && id==ID_1) editor->SetChanBand(-1,0);
      else if (editor->mode==mNothing && id==ID_2) editor->SetChanBand(-1,1);
      else if (editor->mode==mNothing && id==ID_3) editor->SetChanBand(-1,2);
      else if (editor->mode==mNothing && id==ID_4) editor->SetChanBand(-1,3);
      else if (editor->mode==mNothing && id==ID_5) editor->SetChanBand(-1,4);
      else if (editor->mode==mNothing && id==ID_6) editor->SetChanBand(-1,5);
      else if (editor->mode==mNothing && id==ID_NEGATIVE) editor->ToggleNegative();
      else if (editor->mode==mNothing && id==ID_LINETYPE) editor->ToggleLineType();
      else if (editor->mode==mNothing && id==ID_VISIBILITY) editor->SetVisibility(tsWhatever,-1);
      else if (editor->mode==mNothing && id==ID_RED) editor->SetCol(tsWhatever,cRed);
      else if (editor->mode==mNothing && id==ID_GREEN) editor->SetCol(tsWhatever,cGreen);
      else if (editor->mode==mNothing && id==ID_BLUE) editor->SetCol(tsWhatever,cBlue);
      else if (editor->mode==mNothing && id==ID_YELLOW) editor->SetCol(tsWhatever,cYellow);
      else if (editor->mode==mNothing && id==ID_BLACK) editor->SetCol(tsWhatever,cBlack);
      else if (editor->mode==mNothing && id==ID_PURPLE) editor->SetCol(tsWhatever,cPurple);
      else if (editor->mode==mNothing && id==ID_WHITE) editor->SetCol(tsWhatever,cWhite);
      else if (editor->mode==mNothing && id==ID_THIN) editor->SetThickness(0);
      else if (editor->mode==mNothing && id==ID_MEDIUM) editor->SetThickness(1);
      else if (editor->mode==mNothing && id==ID_HEAVY) editor->SetThickness(2);
      else if (editor->mode==mNothing && id==ID_TOFRONT) editor->SetOrder(2);
      else if (editor->mode==mNothing && id==ID_TOBACK) editor->SetOrder(-2);
      else if (editor->mode==mNothing && editor->hit.n>0 && id==ID_0)
      { TLimb &limb=editor->body->limbs[editor->hit.n];
        if (limb.chan==0 || limb.chan==1 || limb.chan==3) editor->SetF(0,limb.chan,limb.band,0);
      }
      //
      editor->UpdateMenus(); editor->SetCursor();
      break;
    } 
    case WM_CLOSE:
    { if (editor->mode==mExporting) {editor->ExportCancelled=true; editor->ExportTerminated=true; return 0;}
      else
      { bool res=editor->FileClose();
        if (!res) return 0;
      }
      break;
    }
    case WM_SIZE:
    { int width=LOWORD(lParam), height=HIWORD(lParam);
      SendMessage(editor->hstatwnd,WM_SIZE,0,lParam);
      RECT rc; GetWindowRect(editor->hstatwnd,&rc); int sheight=rc.bottom-rc.top;
      MoveWindow(editor->chwnd,0,0,width,height-sheight,TRUE);
      break;
    }
  }
  return DefWindowProc(hwnd, msg, wParam, lParam);
}





void RegLoadUser()
{ usercols.clear(); userthick.clear(); usercums.clear(); exportfn="";
  UserSnap=true; UserGrid=false; WhitePrint=false; UserBmpBackground=RGB(128,128,128);
  ShowAngles=true; ShowJoints=true; ShowInvisibles=true;
  UserCheckUpright=true;
  exportfn=""; exportproc=false; exportfigs=""; exporttune=1; exportw=128; exporth=128;
  exporttimefixed=true; exporttime=10000; exportfps=20; exportcompress=true; exportopt=0; exportproctime=4000;
  UserBacks.clear();
  HKEY key=NULL; char dat[10000]; DWORD ddat; DWORD size; DWORD type;
  LONG res=RegOpenKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,KEY_READ,&key);
  if (res==ERROR_SUCCESS)
  { size=sizeof(dat); *dat=0; res=RegQueryValueEx(key,"userbacks",NULL,&type,(LPBYTE)&dat,&size);    if (res==ERROR_SUCCESS) ParseUserBacks(dat);
    size=sizeof(dat); *dat=0; res=RegQueryValueEx(key,"usercols",NULL,&type,(LPBYTE)&dat,&size);     if (res==ERROR_SUCCESS) ParseUserCols(dat);
    size=sizeof(dat); *dat=0; res=RegQueryValueEx(key,"usercums",NULL,&type,(LPBYTE)&dat,&size);     ParseUserCums(dat);
    size=sizeof(dat); *dat=0; res=RegQueryValueEx(key,"userthick",NULL,&type,(LPBYTE)&dat,&size);    if (res==ERROR_SUCCESS) ParseUserThicks(dat);
    size=sizeof(ddat); ddat=1; res=RegQueryValueEx(key,"usersnap",NULL,&type,(LPBYTE)&ddat,&size);   if (res==ERROR_SUCCESS) UserSnap = (ddat!=0);
    size=sizeof(ddat); ddat=0; res=RegQueryValueEx(key,"usergrid",NULL,&type,(LPBYTE)&ddat,&size);   if (res==ERROR_SUCCESS) UserGrid = (ddat!=0);
    size=sizeof(ddat); ddat=0; res=RegQueryValueEx(key,"uservert",NULL,&type,(LPBYTE)&ddat,&size);   if (res==ERROR_SUCCESS) WhitePrint = (ddat!=0);
    size=sizeof(ddat); ddat=128; res=RegQueryValueEx(key,"previewbmpbackground",NULL,&type,(LPBYTE)&ddat,&size); if (res==ERROR_SUCCESS) UserBmpBackground=RGB(ddat,ddat,ddat);
    size=sizeof(ddat); ddat=1; res=RegQueryValueEx(key,"alwayscheckupright",NULL,&type,(LPBYTE)&ddat,&size);     if (res==ERROR_SUCCESS) UserCheckUpright = (ddat!=0);
    size=sizeof(ddat); ddat=1; res=RegQueryValueEx(key,"showjoints",NULL,&type,(LPBYTE)&ddat,&size);             if (res==ERROR_SUCCESS) ShowJoints = (ddat!=0);
    size=sizeof(ddat); ddat=1; res=RegQueryValueEx(key,"showangles",NULL,&type,(LPBYTE)&ddat,&size);             if (res==ERROR_SUCCESS) ShowAngles = (ddat!=0);
    size=sizeof(ddat); ddat=1; res=RegQueryValueEx(key,"showinvisibles",NULL,&type,(LPBYTE)&ddat,&size);         if (res==ERROR_SUCCESS) ShowInvisibles = (ddat!=0);
    //
    size=sizeof(dat); *dat=0; res=RegQueryValueEx(key,"exportfn",NULL,&type,(LPBYTE)&dat,&size);     if (res==ERROR_SUCCESS) exportfn=string(dat);
    size=sizeof(ddat); ddat=0; res=RegQueryValueEx(key,"exportproc",NULL,&type,(LPBYTE)&ddat,&size); if (res==ERROR_SUCCESS) exportproc = (ddat!=0);
    size=sizeof(dat); *dat=0; res=RegQueryValueEx(key,"exportfigs",NULL,&type,(LPBYTE)&dat,&size);   if (res==ERROR_SUCCESS) exportfigs=string(dat);
    size=sizeof(ddat); ddat=1; res=RegQueryValueEx(key,"exporttune",NULL,&type,(LPBYTE)&ddat,&size); if (res==ERROR_SUCCESS) exporttune = ddat;
    size=sizeof(dat); *dat=0; res=RegQueryValueEx(key,"exportwav",NULL,&type,(LPBYTE)&dat,&size);    if (res==ERROR_SUCCESS) exportwav=string(dat);
    size=sizeof(ddat); ddat=128; res=RegQueryValueEx(key,"exportw",NULL,&type,(LPBYTE)&ddat,&size);  if (res==ERROR_SUCCESS) exportw = ddat;
    size=sizeof(ddat); ddat=128; res=RegQueryValueEx(key,"exporth",NULL,&type,(LPBYTE)&ddat,&size);  if (res==ERROR_SUCCESS) exporth = ddat;
    size=sizeof(ddat); ddat=1; res=RegQueryValueEx(key,"exporttimefixed",NULL,&type,(LPBYTE)&ddat,&size);  if (res==ERROR_SUCCESS) exporttimefixed = (ddat!=0);
    size=sizeof(ddat); ddat=10000; res=RegQueryValueEx(key,"exporttime",NULL,&type,(LPBYTE)&ddat,&size);   if (res==ERROR_SUCCESS) exporttime = ddat;
    size=sizeof(ddat); ddat=4000; res=RegQueryValueEx(key,"exportproctime",NULL,&type,(LPBYTE)&ddat,&size);if (res==ERROR_SUCCESS) exportproctime = ddat;
    size=sizeof(ddat); ddat=20; res=RegQueryValueEx(key,"exportfps",NULL,&type,(LPBYTE)&ddat,&size);       if (res==ERROR_SUCCESS) exportfps=ddat;
    size=sizeof(ddat); ddat=1; res=RegQueryValueEx(key,"exportcompress",NULL,&type,(LPBYTE)&ddat,&size);   if (res==ERROR_SUCCESS) exportcompress = (ddat!=0);
    size=sizeof(dat);  ddat=0; res=RegQueryValueEx(key,"exportopt",NULL,&type,(LPBYTE)&ddat,&size);        if (res==ERROR_SUCCESS) exportopt=ddat;
    //
    RegCloseKey(key);
  }
  else
  { TUserColour uc;
    uc.name="Cyan"; uc.c.r=15; uc.c.g=220; uc.c.b=255; usercols.push_back(uc);
    uc.name="Fuchsia"; uc.c.r=255; uc.c.g=10; uc.c.b=180; usercols.push_back(uc);
    uc.name="Silver"; uc.c.r=160; uc.c.g=160; uc.c.b=190; usercols.push_back(uc);
  }
}

void RegSaveUser()
{ HKEY key; DWORD disp; char buf[10000]; DWORD ddat;
  LONG res=RegCreateKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,NULL,0,KEY_WRITE,NULL,&key,&disp);
  if (res==ERROR_SUCCESS)
  { WriteUserBacks(buf,10000); RegSetValueEx(key,"userbacks",0,REG_SZ,(CONST BYTE*)&buf,strlen(buf)+1);
    WriteUserCols(buf,10000); RegSetValueEx(key,"usercols", 0,REG_SZ,(CONST BYTE*)&buf,strlen(buf)+1);
    WriteUserCums(buf,10000); RegSetValueEx(key,"usercums", 0,REG_SZ,(CONST BYTE*)&buf,strlen(buf)+1);
    WriteUserThicks(buf,10000);RegSetValueEx(key,"userthick",0,REG_SZ,(CONST BYTE*)&buf,strlen(buf)+1);
    ddat=UserSnap?1:0; RegSetValueEx(key,"usersnap",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=UserGrid?1:0; RegSetValueEx(key,"usergrid",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=WhitePrint?1:0; RegSetValueEx(key,"uservert",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=GetRValue(UserBmpBackground); RegSetValueEx(key,"previewbmpbackground",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=UserCheckUpright?1:0; RegSetValueEx(key,"alwayscheckupright",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=ShowJoints?1:0; RegSetValueEx(key,"showjoints",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=ShowAngles?1:0; RegSetValueEx(key,"showangles",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=ShowInvisibles?1:0; RegSetValueEx(key,"showinvisibles",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    //
    RegSetValueEx(key,"exportfn",0,REG_SZ,(CONST BYTE*)exportfn.c_str(),exportfn.length()+1);
    ddat=exportproc?1:0;RegSetValueEx(key,"exportproc",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    RegSetValueEx(key,"exportfigs",0,REG_SZ,(CONST BYTE*)exportfigs.c_str(),exportfigs.length()+1);
    ddat=exporttune;   RegSetValueEx(key,"exporttune",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    RegSetValueEx(key,"exportwav",0,REG_SZ,(CONST BYTE*)exportwav.c_str(),exportwav.length()+1);
    ddat=exportw;      RegSetValueEx(key,"exportw",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=exporth;      RegSetValueEx(key,"exporth",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=exporttimefixed?1:0;RegSetValueEx(key,"exporttimefixed",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=exporttime;   RegSetValueEx(key,"exporttime",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=exportproctime;RegSetValueEx(key,"exportproctime",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=exportfps;    RegSetValueEx(key,"exportfps",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=exportcompress?1:0;RegSetValueEx(key,"exportcompress",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    ddat=exportopt;    RegSetValueEx(key,"exportopt",0,REG_DWORD,(CONST BYTE*)&ddat,sizeof(ddat));
    //
    RegCloseKey(key);
  }
}


unsigned int WriteUserCols(char *buf,unsigned int buflen)
{ unsigned int len=0;  if (buf!=0 && buflen>0) *buf=0;
  for (list<TUserColour>::const_iterator i=usercols.begin(); i!=usercols.end(); i++)
  { char c[1000]; wsprintf(c,"%s=RGB(%i,%i,%i)\r\n",i->name.c_str(),i->c.r,i->c.g,i->c.b);
    int cl=strlen(c); if (len+cl<buflen) strcpy(buf+len,c); len+=cl;
  }
  return len;
}

unsigned int WriteUserCums(char *buf,unsigned int buflen)
{ unsigned int len=0; if (buf!=0 && buflen>0) *buf=0;
  if (usercums.size() <= 2) return len;
  list<TUserCum>::const_iterator i=usercums.begin(); i++; i++; // get it past the first two fixed ones
  for (; i!=usercums.end(); i++)
  { char c[100];
    int mul=1;
    const char *rate="rate"; if (i->r<0) {rate="fixedrate"; mul=-1;}
    const char *reflect=""; if (i->reflect) reflect="reflect_";
    sprintf(c,"%s=%s%s(%lg)\r\n",i->name.c_str(),reflect,rate,mul*i->r);
    int cl=strlen(c); if (len+cl<buflen) strcpy(buf+len,c); len+=cl;
  }
  return len;
}


unsigned int WriteUserThicks(char *buf,unsigned int buflen)
{ unsigned int len=0;  if (buf!=0 && buflen>0) *buf=0;
  for (list<double>::const_iterator i=userthick.begin(); i!=userthick.end(); i++)
  { char c[100]; sprintf(c,"%lg\r\n",*i); 
    int cl=strlen(c); if (len+cl<buflen) strcpy(buf+len,c); len+=cl;
  }
  return len;
}


unsigned int WriteUserBacks(char *buf,unsigned int buflen)
{ unsigned int len=0;  if (buf!=0 && buflen>0) *buf=0;
  for (list<TUserBack>::const_iterator i=UserBacks.begin(); i!=UserBacks.end(); i++)
  { char c[MAX_PATH*3]; wsprintf(c,"%s\r\n%s\r\n",i->fn.c_str(),i->back.c_str());
    int cl=strlen(c); if (len+cl<buflen) strcpy(buf+len,c); len+=cl;
  }
  return len;
}



int ParseUserBacks(const char *dat)
{ UserBacks.clear();
  const char *c=dat;
  while (*c!=0)
  { const char *start=c; while (*c!='\r' && *c!='\n' && *c!=0) c++;
    string fn(start,c-start); fn=StringTrim(fn);
    while (*c=='\r' || *c=='\n') c++;
    start=c; while (*c!='\r' && *c!='\n' && *c!=0) c++;
    string back(start,c-start); back=StringTrim(back);
    if (fn!="" && back!="") {TUserBack ub; ub.fn=fn; ub.back=back; UserBacks.push_back(ub);}
    while (*c=='\r' || *c=='\n') c++;
  }
  return -1;
}
  

int ParseUserCols(const char *dat)
{ const char *c=dat;
  list<TUserColour> newcols; int errpos=-1;
  while (*c!=0)
  { while (*c==' ') c++;
    const char *lstart=c;
    while (*c!='=' && *c!='\r' && *c!='\n' && *c!=0) c++;
    const char *leq=c; if (*c!='=') leq=0;
    while (*c!='\r' && *c!='\n' && *c!=0) c++;
    const char *lend=c;
    while (*c=='\r' || *c=='\n') c++;
    errpos=lstart-dat;
    if (lstart!=0 && leq!=0 && lend!=0)
    { TUserColour uc; const char *nstart=lstart, *nend=leq;
      while (*nstart==' ') nstart++; while (nend[-1]==' ') nend--;
      uc.name=string(nstart,nend);
      const char *rgbstart=leq+1; while (*rgbstart==' ') rgbstart++;
      int r,g,b; int res=sscanf(rgbstart,"RGB(%i,%i,%i)",&r,&g,&b);
      if (res==3)
      { uc.c.r=r; uc.c.g=g; uc.c.b=b; newcols.push_back(uc);
        errpos=-1;
      }
    }
    if (errpos!=-1) return errpos;
  }
  usercols = newcols;
  return -1;
}

int ParseUserCums(const char *dat)
{ const char *c=dat;
  list<TUserCum> newcums; int errpos=-1;
  TUserCum uc; uc.name="Noncumulative"; uc.r=0; uc.reflect=0; newcums.push_back(uc);
  uc.name="Cumulative"; uc.r=1; newcums.push_back(uc);
  //
  while (*c!=0)
  { while (*c==' ') c++;  const char *lstart=c;
    while (*c!='=' && *c!='\r' && *c!='\n' && *c!=0) c++; const char *leq=c; if (*c!='=') leq=0;
    while (*c!='\r' && *c!='\n' && *c!=0) c++; const char *lend=c;
    while (*c=='\r' || *c=='\n') c++;
    errpos=lstart-dat;
    if (lstart!=0 && leq!=0 && lend!=0)
    { TUserCum uc; const char *nstart=lstart, *nend=leq;
      while (nend[-1]==' ') nend--; uc.name=string(nstart,nend);
      const char *rstart=leq+1; while (*rstart==' ') rstart++;
      double r=1; bool reflect=false; int res=0;
      if (strncmp(rstart,"rate",4)==0) {res=sscanf(rstart,"rate(%lg)",&r); reflect=false;}
      else if (strncmp(rstart,"fixedrate",9)==0) {res=sscanf(rstart,"fixedrate(%lg)",&r); r=-r; reflect=false;}
      else if (strncmp(rstart,"reflect_rate",12)==0) {res=sscanf(rstart,"reflect_rate(%lg)",&r); reflect=true;}
      else if (strncmp(rstart,"reflect_fixedrate",17)==0) {res=sscanf(rstart,"reflect_fixedrate(%lg)",&r); r=-r; reflect=true;}
      if (res==1) {uc.r=r; uc.reflect=reflect; newcums.push_back(uc); errpos=-1;}
    }
    if (errpos!=-1) return errpos;
  }
  usercums = newcums;
  return -1;
}


int ParseUserThicks(const char *dat)
{ const char *c=dat; list<double> newthick; int errpos=-1;
  while (*c!=0)
  { while (*c==' ') c++;
    const char *lstart=c;
    while (*c!='\r' && *c!='\n' && *c!=0) c++;
    const char *lend=c;
    while (*c=='\r' || *c=='\n') c++;
    errpos = lstart-dat;
    if (lstart!=0 && lend!=0)
    { double d; int res=sscanf(lstart,"%lg",&d);
      if (res==1)
      { userthick.push_back(d); errpos=-1;
      }
    }
    if (errpos!=-1) return errpos;
  }
  userthick = newthick;
  return -1;
}


enum UserDlgMode {userCols, userThicks, userCums} user;
//
int WINAPI UserDlgProc(HWND hdlg,UINT msg,WPARAM wParam,LPARAM lParam)
{ if (msg==WM_INITDIALOG) {SetWindowLong(hdlg,DWL_USER,lParam); user=(UserDlgMode)lParam;}
  else {user=(UserDlgMode)GetWindowLong(hdlg,DWL_USER);}
  //
  switch (msg)
  { case WM_INITDIALOG:
    { if (user==userCols) SetWindowText(hdlg,"User Colours");
      else if (user==userThicks) SetWindowText(hdlg,"User Thicknesses");
      else if (user==userCums) SetWindowText(hdlg,"User Cumulatives");
      //
      if (user==userCols) SetDlgItemText(hdlg,IDC_TITLE,"List one col per line, e.g. blue=RGB(0,100,255) or pink=RGB(255,0,190)");
      else if (user==userThicks) SetDlgItemText(hdlg,IDC_TITLE,"List one thickness per line. 0.0 is thin, 1.0 is medium, 2.0 is thick");
      else if (user==userCums) SetDlgItemText(hdlg,IDC_TITLE,"List one cumulative per line, e.g. cdoublespeed=rate(2.0) or c_rain=fixedrate(0.7) or cx=reflect_rate(3.0) or cz=reflect_fixedrate(1)");
      //
      char buf[10000];
      if (user==userCols) WriteUserCols(buf,10000);
      else if (user==userThicks) WriteUserThicks(buf,10000);
      else if (user==userCums) WriteUserCums(buf,10000);
      SetDlgItemText(hdlg,IDC_EDIT,buf);
      return TRUE;
    }
    case WM_COMMAND:
    { int id=LOWORD(wParam); //code=HIWORD(wParam)
      if (id==IDCANCEL) {EndDialog(hdlg,IDCANCEL); return TRUE;}
      if (id==IDOK)
      { char buf[10000]; GetDlgItemText(hdlg,IDC_EDIT,buf,10000);
        int err=-1;
        if (user==userCols) err=ParseUserCols(buf);
        else if (user==userThicks) err=ParseUserThicks(buf);
        else if (user==userCums) err=ParseUserCums(buf);
        if (err==-1) {RegSaveUser(); EndDialog(hdlg,IDOK); return TRUE;}
        //
        string msg,title;
        if (user==userCols) 
        { title="Unable to read user colours";
          msg  ="There must be one colour listed on each line.\r\nEach colour must have the form\r\n"
                "    colname=RGB(r,g,b)\r\n"
                "where r,g,b are numbers between 0 and 255 inclusive.";
        }
        else if (user==userThicks)
        { title="Unable to read user thicknesses";
          msg = "There must be one user thickness listed on each line.\r\nEach thickness must "
                "be a number.\r\n"
                "    0.0 = as thin as possible\r\n"
                "    1.0 = medium\r\n"
                "    2.0 = heavy";
        }
        else if (user==userCums)
        { title="Unable to read user cumulatives";
          msg = "There must be one cumulative listed on each line.\r\nEach cumulative must be in one of four forms:\r\n"
                "   cname=rate(r)\r\n"
                "   cname=fixedrate(r)\r\n"
                "   cname=reflect_rate(r)\r\n"
                "   cname=reflect_fixedrate(r)\r\n"
                "where r is a number, such as 1.0 for standard speed or 2.0 for double.";
        }
        int errend=err; while (buf[errend]!='\r' && buf[errend]!='\n' && buf[errend]!=0) errend++;
        SendDlgItemMessage(hdlg,IDC_EDIT,EM_SETSEL,err,errend);
        SetFocus(GetDlgItem(hdlg,IDC_EDIT));
        MessageBox(hdlg,msg.c_str(),title.c_str(),MB_OK);
      }
      return TRUE;
    }
  }
  return FALSE;
}


void TEditor::UserColours(HWND hpar)
{ DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_USER),hpar,UserDlgProc,userCols);
}

void TEditor::UserThicknesses(HWND hpar)
{ DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_USER),hpar,UserDlgProc,userThicks);
}

void TEditor::UserCumulatives(HWND hpar)
{ DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_USER),hpar,UserDlgProc,userCums);
}


int WINAPI RenameDlgProc(HWND hdlg,UINT msg,WPARAM wParam,LPARAM lParam)
{ char *buf;
  #pragma warning( push ) 
  #pragma warning( disable : 4244 4312 )
  if (msg==WM_INITDIALOG) {SetWindowLongPtr(hdlg,DWLP_USER,lParam); buf=(char*)lParam;}
  else {buf=(char*)GetWindowLongPtr(hdlg,DWLP_USER);}
  #pragma warning( pop )
  if (buf==0) return FALSE;
  //
  switch (msg)
  { case WM_INITDIALOG:
    { SetDlgItemText(hdlg,IDC_RENAME,buf);
      SetFocus(GetDlgItem(hdlg,IDC_RENAME));
      SendDlgItemMessage(hdlg,IDC_RENAME,EM_SETSEL,0,-1);
      return FALSE;
    }
    case WM_COMMAND:
    { int id=LOWORD(wParam);
      if (id==IDOK) GetDlgItemText(hdlg,IDC_RENAME,buf,MAX_PATH);
      if (id==IDOK || id==IDCANCEL) EndDialog(hdlg,id);
      return TRUE;
    }
  }
  return FALSE;
}
      



LRESULT CALLBACK ListSubclassProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ HWND hpar = GetParent(hwnd); if (hpar==0) return DefWindowProc(hwnd,msg,wParam,lParam);
  LONG_PTR oldproc = GetWindowLongPtr(hpar,GWLP_USERDATA); if (oldproc==0) return DefWindowProc(hwnd,msg,wParam,lParam);
  switch (msg)
  { case WM_RBUTTONDOWN:
    { LRESULT sel = SendMessage(hwnd,LB_ITEMFROMPOINT,0,lParam);
      BOOL isnothing = HIWORD(sel);
      if (isnothing) SendMessage(hwnd,LB_SETCURSEL,(WPARAM)-1,0);
      else SendMessage(hwnd,LB_SETCURSEL,sel,0);
      int id=GetWindowLongPtr(hwnd,GWLP_ID);
      SendMessage(hpar,WM_COMMAND,(WPARAM)(id|(LBN_SELCHANGE<<16)),(LPARAM)hwnd);
      SendMessage(hpar,WM_COMMAND,(WPARAM)(id|(BN_CLICKED<<16)),(LPARAM)hwnd);
    } break;
  }
  return CallWindowProc((WNDPROC)oldproc,hwnd,msg,wParam,lParam);
}


int WINAPI BitmapsDlgProc(HWND hdlg,UINT msg,WPARAM wParam,LPARAM lParam)
{ TEditor *editor;
  #pragma warning( push ) 
  #pragma warning( disable : 4244 4312 )
  if (msg==WM_INITDIALOG) {SetWindowLongPtr(hdlg,DWLP_USER,lParam); editor=(TEditor*)lParam;}
  else {editor=(TEditor*)GetWindowLongPtr(hdlg,DWLP_USER);}
  #pragma warning( pop )
  if (editor==NULL) return FALSE;
  TBody *body = editor->body;
  //
  switch (msg)
  { case WM_INITDIALOG:
    { // subclass the listbox
      HWND hlb = GetDlgItem(hdlg,IDC_LIST);
      LONG_PTR oldproc = GetWindowLongPtr(hlb,GWLP_WNDPROC);
      SetWindowLongPtr(hdlg,GWLP_USERDATA,oldproc);
      SetWindowLongPtr(hlb,GWLP_WNDPROC,(LONG_PTR)ListSubclassProc);
      SendMessage(hdlg,WM_APP,TRUE,0);
      return TRUE;
    }
    case WM_APP:
    { if (wParam)
      { // first reset the contents of the list box
        const char *sel = (const char*)lParam;
        SendDlgItemMessage(hdlg,IDC_LIST,LB_RESETCONTENT,0,0);
        for (vector<TBmp>::const_iterator ci=body->bmps.begin(); ci!=body->bmps.end(); ci++)
        { const char *c = ci->name.c_str();
          SendDlgItemMessage(hdlg,IDC_LIST,LB_ADDSTRING,0,(LPARAM)c);
        }
        if (body->bmps.size()>0)
        { int seli=0;
          if (sel!=0) seli=SendDlgItemMessage(hdlg,IDC_LIST,LB_FINDSTRINGEXACT,(WPARAM)-1,(LPARAM)sel);
          SendDlgItemMessage(hdlg,IDC_LIST,LB_SETCURSEL,seli,0);
        }
      }
      // second ensure that the thing is redrawn
      RECT rc; GetWindowRect(GetDlgItem(hdlg,IDC_PICTURE),&rc);
      ScreenToClient(hdlg,(POINT*)&rc.left); ScreenToClient(hdlg,(POINT*)&rc.right);
      InvalidateRect(hdlg,&rc,FALSE);
      return TRUE;
    } 
    case WM_LBUTTONDOWN:
    { POINT pt; pt.x=LOWORD(lParam); pt.y=HIWORD(lParam);
      RECT rc; GetWindowRect(GetDlgItem(hdlg,IDC_PICTURE),&rc);
      ScreenToClient(hdlg,(POINT*)&rc.left); ScreenToClient(hdlg,(POINT*)&rc.right);
      if (PtInRect(&rc,pt))
      { int c=GetRValue(UserBmpBackground);
        c+=64; if (c==256) c=255; if (c>256) c=0;
        UserBmpBackground=RGB(c,c,c);
        SendMessage(hdlg,WM_APP,FALSE,0);
      }
      return TRUE;
    }
    case WM_PAINT:
    { PAINTSTRUCT ps; BeginPaint(hdlg,&ps);
      RECT rc; GetWindowRect(GetDlgItem(hdlg,IDC_PICTURE),&rc);
      ScreenToClient(hdlg,(POINT*)&rc.left); ScreenToClient(hdlg,(POINT*)&rc.right);
      HBRUSH hbr = CreateSolidBrush(UserBmpBackground);
      FillRect(ps.hdc,&rc,hbr);
      DeleteObject(hbr);
      int seli = SendDlgItemMessage(hdlg,IDC_LIST,LB_GETCURSEL,0,0);
      if (seli!=LB_ERR)
      { char c[MAX_PATH]; SendDlgItemMessage(hdlg,IDC_LIST,LB_GETTEXT,seli,(LPARAM)c);
        for (vector<TBmp>::const_iterator ci=body->bmps.begin(); ci!=body->bmps.end(); ci++)
        { if (_stricmp(ci->name.c_str(),c)==0)
          { HDC tdc = CreateCompatibleDC(ps.hdc);
            BITMAP bmp; GetObject(ci->hbm,sizeof(bmp),&bmp); int bwidth=bmp.bmWidth, bheight=bmp.bmHeight;
            SetStretchBltMode(ps.hdc,COLORONCOLOR);
            int x,y,w,h;
            double fb=((double)bwidth)/((double)bheight), fr=((double)(rc.right-rc.left))/((double)(rc.bottom-rc.top));
            if (fb>fr)
            { x=rc.left; w=rc.right-rc.left;
              double f = ((double)w)/((double)bwidth);
              h=(int)(bheight*f);
              y=rc.top + (rc.bottom-rc.top-h)/2;
            }
            else
            { y=rc.top; h=rc.bottom-rc.top;
              double f = ((double)h)/((double)bheight);
              w=(int)(bwidth*f);
              x=rc.left + (rc.right-rc.left-w)/2;
            }
            // do it with the mask
            HGDIOBJ hold;
            if (ci->hbmask!=0)
            { hold = SelectObject(tdc,ci->hbmask);
              StretchBlt(ps.hdc,x,y,w,h,tdc,0,0,bwidth,bheight,SRCAND);
              SelectObject(tdc,ci->hbm);
              StretchBlt(ps.hdc,x,y,w,h,tdc,0,0,bwidth,bheight,SRCPAINT);
            }
            else
            { hold = SelectObject(tdc,ci->hbm);
              StretchBlt(ps.hdc,x,y,w,h,tdc,0,0,bwidth,bheight,SRCCOPY);
            }
            SelectObject(tdc,hold);
            DeleteDC(tdc);
          }
        }
      }
      EndPaint(hdlg,&ps);
      return TRUE;
    }
    case WM_COMMAND:
    { int id=LOWORD(wParam), code=HIWORD(wParam);
      if (id==IDOK || id==IDCANCEL) EndDialog(hdlg,id);
      else if (id==IDC_LIST && code==LBN_SELCHANGE)
      { SendMessage(hdlg,WM_APP,FALSE,0);
      }
      else if (id==IDC_LIST && code==BN_CLICKED)
      { int seli = SendDlgItemMessage(hdlg,IDC_LIST,LB_GETCURSEL,0,0);
        char c[MAX_PATH];
        if (seli==LB_ERR) *c=0; else SendDlgItemMessage(hdlg,IDC_LIST,LB_GETTEXT,seli,(LPARAM)c);
        string oldname(c);
        //
        enum {rmAdd=101, rmDelete=102, rmRename=13};
        HMENU hrightmenu = CreatePopupMenu();
        if (seli!=LB_ERR)
        { im(hrightmenu,"Rename",rmRename);
          im(hrightmenu,"Delete",rmDelete);
        }
        im(hrightmenu,"Add",rmAdd);
        //
        POINT pt; GetCursorPos(&pt);
        int cmd=TrackPopupMenu(hrightmenu,TPM_LEFTALIGN|TPM_TOPALIGN|TPM_RETURNCMD,pt.x,pt.y,0,hdlg,NULL);
        DestroyMenu(hrightmenu);
        if (cmd==rmDelete)
        { editor->DeleteBitmap(oldname);
          SendMessage(hdlg,WM_APP,TRUE,0);
        }
        else if (cmd==rmRename)
        { int res = DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_RENAME),hdlg,RenameDlgProc,(LPARAM)c);
          if (res==IDOK)
          { string newname(c);
            string zn = editor->RenameBitmap(oldname,newname);
            SendMessage(hdlg,WM_APP,TRUE,(LPARAM)zn.c_str());
          }
        }
        else if (cmd==rmAdd) SendMessage(hdlg,WM_COMMAND,(WPARAM)(IDC_ADD|(BN_CLICKED<<16)),0);
      }
      else if (id==IDC_ADD && code==BN_CLICKED)
      { char tfn[MAX_PATH*20]; *tfn=0;
        OPENFILENAME ofn; ZeroMemory(&ofn,sizeof(ofn));
        ofn.lStructSize=sizeof(ofn);
        ofn.hwndOwner=hdlg;
        ofn.lpstrFilter= "Bitmaps and Jpegs\0*.bmp;*.jpg;*.jpeg\0";
        ofn.lpstrFile=tfn;
        ofn.nMaxFile=MAX_PATH*20;
        ofn.Flags=OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST|OFN_HIDEREADONLY|OFN_ALLOWMULTISELECT|OFN_EXPLORER;
        ofn.lpstrDefExt = "bmp";
        BOOL res = GetOpenFileName(&ofn);
        if (!res) return FALSE;
        string zn="";
        string dir=tfn; const char *fn=tfn+dir.length()+1;
        // either there was just a single filepath, or a dir followed by a list of filenames
        //
        editor->MarkUndo(true);
        if (*fn==0) zn = editor->AddBitmap(dir.c_str());
        else while (*fn!=0)
        { string s = dir+"\\"+fn;
          zn = editor->AddBitmap(s.c_str());
          fn=fn+strlen(fn)+1;
        }
        MakeBindexes(body); 
        editor->ismodified=true;
        editor->Redraw();
        SendMessage(hdlg,WM_APP,TRUE,(LPARAM)zn.c_str());
      }
      return TRUE;
    }
  }
  return FALSE;
}

void TEditor::Bitmaps(HWND hpar)
{ DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_BITMAPS),hpar,BitmapsDlgProc,(LPARAM)this);
}



string TEditor::StyleClick(HWND hpar,const string s)
{ TStyle *style=0; list<TStyle>::iterator styleiterator=body->styles.begin();
  if (s!="")
  { while (styleiterator!=body->styles.end() && StringLower(styleiterator->name)!=StringLower(s)) styleiterator++;
    if (styleiterator!=body->styles.end()) style=&(*styleiterator);
  }
  //
  enum {smDelete=1, smRename=2, smAdd=3,
        smFillVisible=12, smFillInvisible=13, smFillAlternate=14, smFillWinding=15, smFillMore=16, smFill=17, smFillLast=49,
        smLineVisible=50, smLineInvisible=51, smLineMore=52, smLineExtra2=53, smLineExtra1=54, smLineThick=55, smLineMedium=56, smLineThin=57, smLine=58, smLineLast=99,
        smFreqFixed=111, smFreqNegative=113, smFreqLeft=114, smFreqRight=125, smFreqDifference=135, smFreqKaraoke=145,
        smMoreBitmaps=180, smBitmaps=181, smBitmapsLast=250,
        smShortNone=251,smShortFirst=252,smShortLast=299,
        smMoreEffects=300, smLineEffectsFirst=301, smLineEffectsLast=399,
        smFillEffectsFirst=400, smFillEffectsLast=499,
        smMoreCums=500,smCumsFirst=501,smCumsLast=599};
  HMENU hstylemenu=CreatePopupMenu();
  HMENU hlinemenu=CreatePopupMenu(), hfillmenu=CreatePopupMenu(), hfreqmenu=CreatePopupMenu();
  HMENU hshortmenu=CreatePopupMenu();
  vector<COLORREF> extracols; 
  MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi); 
  //
  // styles
  im(hstylemenu,"Add",smAdd);
  if (style!=0)
  { im(hstylemenu,"Delete",smDelete);
    im(hstylemenu,"Rename",smRename);
  }
  //
  // shortcut
  if (style!=0)
  { vector<bool> available; available.resize(20,true);
    for (list<TStyle>::const_iterator i=body->styles.begin(); i!=body->styles.end(); i++)
    { if (i->shortcut!=0) available[i->shortcut-'A']=false;
    }
    int shortcut=-1;
    if (style->shortcut!=0) {shortcut=style->shortcut-'A'; available[shortcut]=true;}
    im(hshortmenu,"None",smShortNone,shortcut==-1,true,true);
    for (int i=19; i>=0; i--)
    { char c = (char)('A'+i);
      string s = "Ctrl+"+string(&c,1);
      if (c!='C') im(hshortmenu,s,smShortFirst+i,shortcut==i,available[i],true);
    }
    im(hstylemenu,"Shortcut",hshortmenu);
    im(hstylemenu,"--");
  }
  //
  // Fill 
  if (style!=0)
  { bool isvisible = (style->shape.brush.type!=ctNone);
    list<COLORREF> cols; if (style->shape.brush.type==ctRGB) cols.push_back(CREF(style->shape.brush.rgb));
    list<string> effects; if (style->limb.color.type==ctEffect) effects.push_back(style->limb.color.effect);
    //
    im(hfillmenu,"Effects...",smMoreEffects);
    im(hfillmenu,body->effects,effects,style->shape.brush.type!=ctEffect,smFillEffectsFirst);
    im(hfillmenu,"--");
    im(hfillmenu,"Bitmaps...",smMoreBitmaps);
    for (int bic=0; bic<(int)body->bmps.size(); bic++)
    { string bname = body->bmps[bic].name;
      bool checked = (StringLower(bname)==StringLower(style->shape.brush.bitmap));
      checked &= (style->shape.brush.type==ctBitmap);
      im(hfillmenu,bname,smBitmaps+bic,checked,true,true);
    }
    im(hfillmenu,"--");
    im(hfillmenu,"Colours...",smFillMore);
    im(hfillmenu,cols,style->shape.brush.type!=ctRGB,smFill,false,&extracols);
    im(hfillmenu,"--");
    im(hfillmenu,"Invisible",isvisible?smFillInvisible:smFillVisible,!isvisible,true,false);
    im(hstylemenu,"Fill",hfillmenu);
  }
  //
  // Line
  if (style!=0)
  { list<COLORREF> cols;  if (style->limb.color.type==ctRGB) cols.push_back(CREF(style->limb.color.rgb));
    list<string> effects; if (style->limb.color.type==ctEffect) effects.push_back(style->limb.color.effect);
    int thick; if (style->limb.thickness<0.5) thick=0; else if (style->limb.thickness<1.5) thick=1; 
    else if (style->limb.thickness<3) thick=2; else if (style->limb.thickness<6) thick=3; else thick=4;
    if (style->limb.color.type==ctNone) thick=-1;
    //
    im(hlinemenu,"Effects...",smMoreEffects);
    im(hlinemenu,body->effects,effects,style->limb.color.type!=ctEffect,smLineEffectsFirst,imDisallowBitmaps);
    im(hlinemenu,"--");
    im(hlinemenu,"Colours...",smLineMore);
    im(hlinemenu,cols,style->limb.color.type!=ctRGB,smLine,false,&extracols);
    im(hlinemenu,"--");
    im(hlinemenu,"Extra2",smLineExtra2,thick==4);
    im(hlinemenu,"Extra",smLineExtra1,thick==3);
    im(hlinemenu,"Heavy",smLineThick,thick==2);
    im(hlinemenu,"Medium",smLineMedium,thick==1);
    im(hlinemenu,"Thin",smLineThin,thick==0);
    im(hlinemenu,"--");
    im(hlinemenu,"Invisible",style->limb.color.type==ctNone?smLineVisible:smLineInvisible,style->limb.color.type==ctNone,true,false);
    im(hstylemenu,"Line",hlinemenu);
  }
  //
  // Frequency
  vector<CumInfo> retcums;
  if (style!=0)
  { int chan=style->limb.chan, band=style->limb.band;
    list<CumInfo> cums; if (style->limb.cum) cums.push_back(CumInfo(style->limb.crate,style->limb.creflect)); else cums.push_back(CumInfo(0,0));
    //
    im(hfreqmenu,"Cumulatives...",smMoreCums);
    im(hfreqmenu,cums,smCumsFirst,retcums);
    im(hfreqmenu,"Negative",smFreqNegative,style->limb.negative,true,false);
    im(hfreqmenu,"--");
    im(hfreqmenu,"Music", smFreqKaraoke+1, chan==3&&band==1);
    im(hfreqmenu,"Vocals",smFreqKaraoke+0, chan==3&&band==0);
    im(hfreqmenu,"--");
    im(hfreqmenu,"Difference: 6",smFreqDifference+5, chan==2&&band==5);
    im(hfreqmenu,"Difference: 5",smFreqDifference+4, chan==2&&band==4);
    im(hfreqmenu,"Difference: 4",smFreqDifference+3, chan==2&&band==3);
    im(hfreqmenu,"Difference: 3",smFreqDifference+2, chan==2&&band==2);
    im(hfreqmenu,"Difference: 2",smFreqDifference+1, chan==2&&band==1);
    im(hfreqmenu,"Difference: 1",smFreqDifference+0, chan==2&&band==0);
    im(hfreqmenu,"--");
    im(hfreqmenu,"Right: 6 treble",smFreqRight+5, chan==1&&band==5);
    im(hfreqmenu,"Right: 5",smFreqRight+4, chan==1&&band==4);
    im(hfreqmenu,"Right: 4",smFreqRight+3, chan==1&&band==3);
    im(hfreqmenu,"Right: 3",smFreqRight+2, chan==1&&band==2);
    im(hfreqmenu,"Right: 2",smFreqRight+1, chan==1&&band==1);
    im(hfreqmenu,"Right: 1 bass",smFreqRight+0, chan==1&&band==0);
    im(hfreqmenu,"--");
    im(hfreqmenu,"Left: 6 treble",smFreqLeft+5, chan==0&&band==5);
    im(hfreqmenu,"Left: 5",smFreqLeft+4, chan==0&&band==4);
    im(hfreqmenu,"Left: 4",smFreqLeft+3, chan==0&&band==3);
    im(hfreqmenu,"Left: 3",smFreqLeft+2, chan==0&&band==2);
    im(hfreqmenu,"Left: 2",smFreqLeft+1, chan==0&&band==1);
    im(hfreqmenu,"Left: 1 bass",smFreqLeft+0, chan==0&&band==0);
    im(hfreqmenu,"--");
    im(hfreqmenu,"Fixed",smFreqFixed, chan==4&&band==0);
    im(hstylemenu,"Frequency",hfreqmenu);
  }
  //
  
  POINT pt; GetCursorPos(&pt);
  BalanceMenu(hstylemenu); BalanceMenu(hfreqmenu);
  BalanceMenu(hlinemenu); BalanceMenu(hfillmenu);
  int cmd=TrackPopupMenu(hstylemenu,TPM_LEFTALIGN|TPM_TOPALIGN|TPM_RETURNCMD,pt.x,pt.y,0,hpar,NULL);
  DestroyMenu(hstylemenu);
  DestroyMenu(hfreqmenu);
  DestroyMenu(hlinemenu);
  DestroyMenu(hfillmenu);
  DestroyMenu(hshortmenu);
  if (cmd==0) return s;
  
  //
  MarkUndo();
  ismodified=true;
 
  if (cmd==smLineVisible && style->limb.color.type==ctNone) style->limb.color.type=style->limb.color.otype;
  else if (cmd==smShortNone) {style->shortcut=0; return style->name;}
  else if (cmd>=smShortFirst && cmd<=smShortLast) {style->shortcut=(char)(cmd-smShortFirst+'A'); return style->name;}
  else if (cmd==smAdd)
  { string name=CleanStyleName("NewStyle");
    TStyle s; s.name=name; body->styles.push_back(s);
    return name;
  }
  else if (cmd==smRename || cmd==smDelete)
  { string lcoldname=StringLower(style->name),newname("");
    if (cmd==smRename)
    { char c[100]; strcpy(c,style->name.c_str());
      int res = DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_RENAME),hpar,RenameDlgProc,(LPARAM)c);
      if (res!=IDOK) return style->name;
      newname=CleanStyleName(c);
    }
    for (int i=0; i<body->nlimbs; i++)
    { if (StringLower(body->limbs[i].linestyle)==lcoldname) body->limbs[i].linestyle=newname;
      if (StringLower(body->limbs[i].freqstyle)==lcoldname) body->limbs[i].freqstyle=newname;
    }
    for (vector<stk::TShape>::iterator si=body->shapes.begin(); si!=body->shapes.end(); si++)
    { if (StringLower(si->fillstyle)==lcoldname) si->fillstyle=newname;
      if (StringLower(si->linestyle)==lcoldname) si->linestyle=newname;
    }
    style->name=newname;
    if (cmd==smDelete)
    { body->styles.erase(styleiterator);
    }
    return newname;
  }
  else if (cmd==smLineInvisible) {if (style->limb.color.type!=ctNone) style->limb.color.otype=style->limb.color.type; style->limb.color.type=ctNone;}
  else if (cmd==smFillVisible && style->shape.brush.type==ctNone) style->shape.brush.type=style->shape.brush.otype;
  else if (cmd==smFillInvisible) {if (style->shape.brush.type!=ctNone) style->shape.brush.otype=style->shape.brush.type; style->shape.brush.type=ctNone;}
  else if (cmd==smFillMore||cmd==smLineMore) {UserColours(hpar); return style->name;}
  else if (cmd==smMoreBitmaps) {Bitmaps(hpar); return style->name;}
  else if (cmd>=smBitmaps && cmd<=smBitmapsLast) {style->shape.brush.type=ctBitmap; style->shape.brush.bitmap=body->bmps[cmd-smBitmaps].name;}
  else if (cmd==smFreqNegative) style->limb.negative = !style->limb.negative;
  else if (cmd>=smFreqDifference && cmd<=smFreqDifference+5) {style->limb.chan=2; style->limb.band=cmd-smFreqDifference;}
  else if (cmd>=smFreqLeft && cmd<=smFreqLeft+5) {style->limb.chan=0; style->limb.band=cmd-smFreqLeft;}
  else if (cmd>=smFreqRight && cmd<=smFreqRight+5) {style->limb.chan=1; style->limb.band=cmd-smFreqRight;}
  else if (cmd>=smFreqKaraoke && cmd<=smFreqKaraoke+5) {style->limb.chan=3; style->limb.band=cmd-smFreqKaraoke;}
  else if (cmd==smFreqFixed) {style->limb.chan=4; style->limb.band=0;}
  else if (cmd==smLineExtra2) {style->limb.thickness=8.0; if (style->limb.color.type==ctNone) style->limb.color.type=style->limb.color.otype;}
  else if (cmd==smLineExtra1) {style->limb.thickness=4.0; if (style->limb.color.type==ctNone) style->limb.color.type=style->limb.color.otype;}
  else if (cmd==smLineThick) {style->limb.thickness=2.0; if (style->limb.color.type==ctNone) style->limb.color.type=style->limb.color.otype;}
  else if (cmd==smLineMedium) {style->limb.thickness=1.0; if (style->limb.color.type==ctNone) style->limb.color.type=style->limb.color.otype;}
  else if (cmd==smLineThin) {style->limb.thickness=0.0; if (style->limb.color.type==ctNone) style->limb.color.type=style->limb.color.otype;}
  else if (cmd==smMoreEffects) Effects(hpar);
  else if (cmd>=smLineEffectsFirst && cmd<=smLineEffectsLast) {style->limb.color.type=ctEffect; style->limb.color.effect=body->effects[cmd-smLineEffectsFirst].name;}
  else if (cmd>=smFillEffectsFirst && cmd<=smFillEffectsLast) {style->shape.brush.type=ctEffect; style->shape.brush.effect=body->effects[cmd-smFillEffectsFirst].name;}
  else if (cmd==smMoreCums) UserCumulatives(hpar);
  else if (cmd>=smCumsFirst && cmd<=smCumsLast)
  { double d = retcums[cmd-smCumsFirst].d;
    int r = retcums[cmd-smCumsFirst].r;
    if (d==0) {style->limb.cum=false;}
    else {style->limb.cum=true; style->limb.crate=d; style->limb.creflect=r;}
  }
  else if ((cmd>=smLine && cmd<=smLineLast) || (cmd>=smFill && cmd<=smFillLast))
  { TSetSubject subj=tsLine; int c=cmd-smLine;
    if (cmd>=smFill && cmd<=smFillLast) {subj=tsFill; c=cmd-smFill;}
    int r,g,b;
    if (c<ncols)
    { r=defcols[c].r; g=defcols[c].g; b=defcols[c].b;
    }
    else if (c<ncols+(int)usercols.size())
    { int ci=ncols; list<TUserColour>::const_iterator ui=usercols.begin();
      while (ci<c) {ci++; ui++;}
      r=ui->c.r; g=ui->c.g; b=ui->c.b;
    }
    else
    { int e=c-ncols-(int)usercols.size(); COLORREF cl=extracols[e];
      r=GetRValue(cl); g=GetGValue(cl); b=GetBValue(cl);
    }
    if (subj==tsLine) {style->limb.color.type=ctRGB; style->limb.color.rgb=RGBCOLOR(r,g,b);}
    else {style->shape.brush.type=ctRGB; style->shape.brush.rgb=RGBCOLOR(r,g,b);}
  }
  //
  // And now propogate the changes through the document
  ApplyStyle(style->name);
  Redraw(); SetCursor();
  //
  return style->name;
}

TStyle TEditor::StyleFromName(const string name)
{ if (name=="") {TStyle s; s.name=""; return s;}
  string lcname=StringLower(name);
  list<TStyle>::const_iterator si=body->styles.begin();
  while (si!=body->styles.end() && StringLower(si->name)!=lcname) si++;
  if (si==body->styles.end()) {TStyle s; s.name=""; return s;}
  else return *si;
}

void TEditor::ApplyStyle(const string name,bool suppressredraw)
{ bool anybmp=false;
  string lcname = StringLower(name);
  list<TStyle>::const_iterator si=body->styles.begin();
  while (si!=body->styles.end() && StringLower(si->name)!=lcname) si++;
  if (si==body->styles.end()) return;
  const TStyle &style = *si;
  //
  for (int i=0; i<body->nlimbs; i++)
  { TLimb &limb = body->limbs[i];
    if (StringLower(limb.linestyle)==lcname)
    { limb.color=style.limb.color;
      limb.thickness=style.limb.thickness;
    }
    if (StringLower(limb.freqstyle)==lcname)
    { limb.chan=style.limb.chan; limb.band=style.limb.band; limb.negative=style.limb.negative;
      limb.cum=style.limb.cum; limb.crate=style.limb.crate; limb.creflect=style.limb.creflect;
      RepositioningMove(i,b2x(limb.x),b2y(limb.y),true);
    }
  }
  for (int si=0; si<(int)body->shapes.size(); si++)
  { stk::TShape &shape = body->shapes[si];
    if (StringLower(shape.linestyle)==lcname)
    { shape.line=style.limb.color;
      shape.thickness=style.limb.thickness;
    }
    if (StringLower(shape.fillstyle)==lcname)
    { if (style.shape.brush.type!=ctBitmap) {shape.brush=style.shape.brush;}
      else
      { // we can't assign a bitmap to anything other than a circle
        int li = circlelimb_from_shape(si);
        if (li!=-1) {shape.brush=style.shape.brush; anybmp=true;}
      }
    }
  }
  if (!suppressredraw)
  { if (anybmp) MakeBindexes(body);
    MakeEindexes(body);
    Recalc(0);
  }
}

string TEditor::CleanStyleName(const string s)
{ string broot=s;
  for (int i=0; i<(int)broot.size(); i++)
  { if (broot[i]==')') broot[i]=']';
    if (broot[i]=='(') broot[i]='[';
    if (broot[i]=='=') broot[i]='-';
  }
  for (int i=1; ; i++)
  { string bname;
    if (i==1) bname=broot; else bname=broot+StringInt(i);
    bool okay=true;
    for (list<TStyle>::const_iterator si=body->styles.begin(); si!=body->styles.end() && okay; si++)
    { if (StringLower(si->name)==StringLower(bname)) okay=false;
    }
    if (okay) return bname;
  }
}



int WINAPI StylesDlgProc(HWND hdlg,UINT msg,WPARAM wParam,LPARAM lParam)
{ TEditor *editor;
  #pragma warning( push ) 
  #pragma warning( disable : 4244 4312 )
  if (msg==WM_INITDIALOG) {SetWindowLongPtr(hdlg,DWLP_USER,lParam); editor=(TEditor*)lParam;}
  else {editor=(TEditor*)GetWindowLongPtr(hdlg,DWLP_USER);}
  #pragma warning( pop )
  if (editor==NULL) return FALSE;
  TBody *body = editor->body;
  //
  switch (msg)
  { case WM_INITDIALOG:
    { // subclass the listbox
      HWND hlb = GetDlgItem(hdlg,IDC_LIST);
      LONG_PTR oldproc = GetWindowLongPtr(hlb,GWLP_WNDPROC);
      SetWindowLongPtr(hdlg,GWLP_USERDATA,oldproc);
      SetWindowLongPtr(hlb,GWLP_WNDPROC,(LONG_PTR)ListSubclassProc);
      SendMessage(hdlg,WM_APP,0,0);
      return TRUE;
    }
    case WM_APP:
    { // reset the contents of the list box
      const char *csel = (const char*)lParam;
      SendDlgItemMessage(hdlg,IDC_LIST,LB_RESETCONTENT,0,0);
      for (list<TStyle>::const_iterator si=body->styles.begin(); si!=body->styles.end(); si++)
      { string s = si->tostring();
        SendDlgItemMessage(hdlg,IDC_LIST,LB_ADDSTRING,0,(LPARAM)s.c_str());
      }
      if (body->styles.size()>0)
      { int seli=0;
        if (csel!=0) seli=SendDlgItemMessage(hdlg,IDC_LIST,LB_FINDSTRING,(WPARAM)-1,(LPARAM)(string(csel)+"=").c_str());
        SendDlgItemMessage(hdlg,IDC_LIST,LB_SETCURSEL,seli,0);
      }
      return TRUE;
    } 
    case WM_COMMAND:
    { int id=LOWORD(wParam), code=HIWORD(wParam);
      if (id==IDOK || id==IDCANCEL) EndDialog(hdlg,id);
      else if (id==IDC_LIST && code==BN_CLICKED)
      { int seli = SendDlgItemMessage(hdlg,IDC_LIST,LB_GETCURSEL,0,0);
        string oldname="";
        if (seli!=LB_ERR)
        { int len = SendDlgItemMessage(hdlg,IDC_LIST,LB_GETTEXTLEN,seli,0);
          char *c=new char[len+1]; SendDlgItemMessage(hdlg,IDC_LIST,LB_GETTEXT,seli,(LPARAM)c);
          char *eq=strchr(c,'='); if (eq!=0) *eq=0;
          oldname=c; delete[] c;
        }
        const string newname = editor->StyleClick(hdlg,oldname);
        SendMessage(hdlg,WM_APP,0,(LPARAM)newname.c_str());
      }
      return TRUE;
    }
  }
  return FALSE;
}



void TEditor::Styles(HWND hpar)
{ DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_STYLES),hpar,StylesDlgProc,(LPARAM)this);
}




vector<TEffect>::const_iterator get_effect(const vector<TEffect> &effects,const string name)
{ vector<TEffect>::const_iterator ei = effects.begin();
  while (ei!=effects.end() && StringLower(ei->name)!=StringLower(name)) ei++;
  return ei;
}

vector<TEffect>::iterator get_effect(vector<TEffect> &effects,const string name)
{ vector<TEffect>::iterator ei = effects.begin();
  while (ei!=effects.end() && StringLower(ei->name)!=StringLower(name)) ei++;
  return ei;
}

struct EffectsDlgData
{ EffectsDlgData() : editor(0), editup(false), dragging(-2) {}
  TEditor *editor; bool editup;
  unsigned int timeStart,timePrev; int samplei;
  // the following is used only internally for EffectWndProc
  string seffect; // [in/out]
  TEffect effect;
  RECT rc; int w,h; // rc is the rect of the segmented bar.
  int knobx,knoby; 
  int dragging; // 0..n-1=line to right of segment. -1=knob. -2=nothing.
};


LRESULT CALLBACK EffectWndProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ EffectsDlgData *dat;
  #pragma warning( push ) 
  #pragma warning( disable : 4244 4312 )
  if (msg==WM_CREATE) {CREATESTRUCT *cs=(CREATESTRUCT*)lParam; SetWindowLongPtr(hwnd,GWLP_USERDATA,(LONG_PTR)cs->lpCreateParams); dat=(EffectsDlgData*)cs->lpCreateParams;}
  else {dat=(EffectsDlgData*)GetWindowLongPtr(hwnd,GWLP_USERDATA);}
  #pragma warning( pop )
  if (dat==NULL) return DefWindowProc(hwnd,msg,wParam,lParam);
  TEffect &effect = dat->effect;
  TEditor *editor = dat->editor;
  TBody *body = dat->editor->body;
  //
  switch (msg)
  { case WM_CREATE: 
    { dat->effect.fromstring(dat->seffect);
      RECT rc; GetClientRect(hwnd,&rc); dat->w=rc.right; dat->h=rc.bottom;
      dat->rc.left=dat->w/6; dat->rc.top=dat->h/6; dat->rc.right=dat->w*5/6; dat->rc.bottom=dat->h*4/6;
      dat->knoby=dat->h*5/6;
      double dx=effect.f*(double)dat->w;
      dat->knobx=dat->rc.left+(int)dx; 
    } return 0;
    case WM_COMMAND:
    { int id=LOWORD(wParam);
      if (id==IDOK) {dat->seffect=effect.tostring(); DestroyWindow(hwnd);}
    } return 0;
    case WM_PAINT:
    { PAINTSTRUCT ps; BeginPaint(hwnd,&ps);
      double tot=0; for (unsigned int segment=0; segment<effect.cols.size(); segment++)
      { double nexttot=tot+effect.fracs[segment]; if (segment==effect.cols.size()-1) nexttot=1;
        stk::TColor col=effect.cols[segment], nextcol=effect.cols[(segment+1)%effect.cols.size()];
        double dw=dat->rc.right-dat->rc.left;
        int x0=dat->rc.left+(int)(tot*dw), x1=dat->rc.left+(int)(nexttot*dw);
        RECT rc; rc.left=x0; rc.top=dat->rc.top; rc.right=x1; rc.bottom=dat->rc.bottom;
        tot=nexttot;
        //
        if (col.type==ctNone || col.type==ctBitmap) FillRect(ps.hdc,&rc,(HBRUSH)GetStockObject(BLACK_BRUSH));
        else if (col.type==ctRGB)
		{
			LOGBRUSH lbr; lbr.lbStyle = BS_SOLID; lbr.lbColor = RGB(col.rgb.r, col.rgb.g, col.rgb.b);
          if (col.fadetonext && nextcol.type==ctRGB)
          { for (int x=x0; x<x1; x+=5)
            { double r1=col.rgb.r, g1=col.rgb.g, b1=col.rgb.b;
              double r2=nextcol.rgb.r, g2=nextcol.rgb.g, b2=nextcol.rgb.b;
              double f = ((double)(x-x0))/((double)(x1-x0));
              double r=r1*(1-f)+r2*f, g=g1*(1-f)+g2*f, b=b1*(1-f)+b2*f;
              lbr.lbColor=RGB((int)r,(int)g,(int)b);
              HBRUSH hbr=CreateBrushIndirect(&lbr);
              RECT src; src.left=x; src.top=rc.top; src.right=x+5; src.bottom=rc.bottom; if (src.right>rc.right) src.right=rc.right;
              FillRect(ps.hdc,&src,hbr);
              DeleteObject(hbr);
            }
          }
          else
          { lbr.lbColor=RGB(col.rgb.r,col.rgb.g,col.rgb.b);
            HBRUSH hbr=CreateBrushIndirect(&lbr);
            FillRect(ps.hdc,&rc,hbr);
            DeleteObject(hbr);
          }
        }
        else LUASSERTMM("unknown col type");
        //
        if (col.type==ctNone)
        { SelectObject(ps.hdc,GetStockObject(WHITE_PEN));
          MoveToEx(ps.hdc,rc.left,rc.top,0); LineTo(ps.hdc,rc.right,rc.bottom);
          MoveToEx(ps.hdc,rc.right,rc.top,0); LineTo(ps.hdc,rc.left,rc.bottom);
        }
        if (col.type==ctBitmap)
        { HDC tdc = CreateCompatibleDC(ps.hdc);
          int bindex=body->bindex(col.bitmap); LUASSERT(bindex!=-1);
          const TBmp &bitmap = body->bmps[bindex];
          BITMAP bmp; GetObject(bitmap.hbm,sizeof(bmp),&bmp); int bwidth=bmp.bmWidth, bheight=bmp.bmHeight;
          SetStretchBltMode(ps.hdc,COLORONCOLOR);
          int x,y,w,h;
          double fb=((double)bwidth)/((double)bheight), fr=((double)(rc.right-rc.left))/((double)(rc.bottom-rc.top));
          if (fb>fr)
          { x=rc.left; w=rc.right-rc.left;
            double f = ((double)w)/((double)bwidth);
            h=(int)(bheight*f);
            y=rc.top + (rc.bottom-rc.top-h)/2;
          }
          else
          { y=rc.top; h=rc.bottom-rc.top;
            double f = ((double)h)/((double)bheight);
            w=(int)(bwidth*f);
            x=rc.left + (rc.right-rc.left-w)/2;
          }
          // do it with the mask
          HGDIOBJ hold;
          if (bitmap.hbmask!=0)
          { hold = SelectObject(tdc,bitmap.hbmask);
            StretchBlt(ps.hdc,x,y,w,h,tdc,0,0,bwidth,bheight,SRCAND);
            SelectObject(tdc,bitmap.hbm);
            StretchBlt(ps.hdc,x,y,w,h,tdc,0,0,bwidth,bheight,SRCPAINT);
          }
          else
          { hold = SelectObject(tdc,bitmap.hbm);
            StretchBlt(ps.hdc,x,y,w,h,tdc,0,0,bwidth,bheight,SRCCOPY);
          }
          SelectObject(tdc,hold);
          DeleteDC(tdc);
        }
      }
      //
      tot=0; for (unsigned int segment=0; segment<effect.cols.size()-1; segment++)
      { double dw=dat->rc.right-dat->rc.left; 
        tot+=effect.fracs[segment]; int x=dat->rc.left+(int)(tot*dw);
        RECT rc; rc.left=x-2; rc.right=x+2; rc.top=dat->rc.top; rc.bottom=dat->rc.bottom;
        FillRect(ps.hdc,&rc,(HBRUSH)GetStockObject(WHITE_BRUSH));
        rc.left++; rc.right--; rc.top++; rc.bottom--;
        FillRect(ps.hdc,&rc,(HBRUSH)GetStockObject(BLACK_BRUSH));
      }
      //
      RECT rc;
      rc.left=dat->rc.left; rc.top=dat->knoby-1; rc.right=dat->knobx; rc.bottom=dat->knoby+1;
      FillRect(ps.hdc,&rc,(HBRUSH)GetStockObject(WHITE_BRUSH));
      rc.left=dat->knobx-4; rc.right=rc.left+8; rc.top=dat->knoby-4; rc.bottom=rc.top+8;
      FillRect(ps.hdc,&rc,(HBRUSH)GetStockObject(WHITE_BRUSH));
      rc.left=dat->knobx-2; rc.right=rc.left+4; rc.top=dat->knoby-2; rc.bottom=rc.top+4;
      FillRect(ps.hdc,&rc,(HBRUSH)GetStockObject(BLACK_BRUSH));
      //
      SelectObject(ps.hdc,GetStockObject(WHITE_PEN));
      SelectObject(ps.hdc,GetStockObject(NULL_BRUSH));
      Rectangle(ps.hdc,dat->rc.left,dat->rc.top,dat->rc.right,dat->rc.bottom);
      //
      EndPaint(hwnd,&ps);
    } return 0;
    case WM_LBUTTONDOWN: case WM_RBUTTONDOWN: case WM_SETCURSOR:
    { int x=LOWORD(lParam),y=HIWORD(lParam);
      if (msg==WM_SETCURSOR) {POINT pt; GetCursorPos(&pt); ScreenToClient(hwnd,&pt); x=pt.x; y=pt.y;}
      int sel=-2; bool online=false;
      if (x>=dat->rc.left && x<dat->knobx+4 && y>=dat->knoby-4 && y<dat->knoby+4)
      { sel=-1; online=(x>=dat->knobx-4);
      }
      else if (x>=dat->rc.left && x<dat->rc.right+4 && y>=dat->rc.top && y<dat->rc.bottom)
      { double tot=0; for (unsigned int segment=0; segment<effect.cols.size(); segment++)
        { double dw=dat->rc.right-dat->rc.left;
          tot+=effect.fracs[segment]; if (segment==effect.cols.size()-1) tot=1;
          int sx1=dat->rc.left+(int)(tot*dw);
          if (x<sx1+4) {sel=(int)segment; online=(x>=sx1-4); break;}
        }
      }
      if (msg==WM_SETCURSOR) 
      { if (online) SetCursor(LoadCursor(NULL,IDC_SIZEWE));
        else SetCursor(LoadCursor(NULL,IDC_ARROW));
        return TRUE;
      }
      if (sel==-2) return 0;
      if (msg==WM_LBUTTONDOWN && sel==(int)effect.cols.size()-1) return 0;
      if (msg==WM_LBUTTONDOWN && online) {dat->dragging=sel; return 0;}
      if (msg==WM_LBUTTONDOWN && sel==-1) {dat->dragging=sel; return 0;}
      if (msg==WM_LBUTTONDOWN) return 0;
      //
      enum {rmFreqNegative=113, rmFreqLeft=114, rmFreqRight=125, rmFreqDifference=135,
            rmFreqCumMore=151,rmFreqCumFirst=152, rmFreqCumLast=200,
            rmMoreBitmaps=201, rmBitmaps=202, rmColMore=300, rmColFade=301, rmCol=302,
            rmAddAfter=403,rmAddBefore=404,rmDelete=405,
            rmFreqKaraoke=406,rmInvisible=420};
      vector<COLORREF> extracols; 
      HMENU hrightmenu=CreatePopupMenu();
      vector<CumInfo> retcums;
      if (sel==-1)
      { list<CumInfo> cums; if (effect.cumulative) cums.push_back(CumInfo(effect.cumrate,effect.creflect)); else cums.push_back(CumInfo(0,0));
        im(hrightmenu,"Cumulatives...",rmFreqCumMore);
        im(hrightmenu,cums,rmFreqCumFirst,retcums);
        im(hrightmenu,"Negative",rmFreqNegative,effect.negative,true,false);
        im(hrightmenu,"--");
        im(hrightmenu,"Music", rmFreqKaraoke+1, effect.chan==3&&effect.band==1);
        im(hrightmenu,"Vocals",rmFreqKaraoke+0, effect.chan==3&&effect.band==0);
        im(hrightmenu,"--");
        im(hrightmenu,"Difference: 6",rmFreqDifference+5, effect.chan==2&&effect.band==5);
        im(hrightmenu,"Difference: 5",rmFreqDifference+4, effect.chan==2&&effect.band==4);
        im(hrightmenu,"Difference: 4",rmFreqDifference+3, effect.chan==2&&effect.band==3);
        im(hrightmenu,"Difference: 3",rmFreqDifference+2, effect.chan==2&&effect.band==2);
        im(hrightmenu,"Difference: 2",rmFreqDifference+1, effect.chan==2&&effect.band==1);
        im(hrightmenu,"Difference: 1",rmFreqDifference+0, effect.chan==2&&effect.band==0);
        im(hrightmenu,"--");
        im(hrightmenu,"Right: 6 treble",rmFreqRight+5, effect.chan==1&&effect.band==5);
        im(hrightmenu,"Right: 5",rmFreqRight+4, effect.chan==1&&effect.band==4);
        im(hrightmenu,"Right: 4",rmFreqRight+3, effect.chan==1&&effect.band==3);
        im(hrightmenu,"Right: 3",rmFreqRight+2, effect.chan==1&&effect.band==2);
        im(hrightmenu,"Right: 2",rmFreqRight+1, effect.chan==1&&effect.band==1);
        im(hrightmenu,"Right: 1 bass",rmFreqRight+0, effect.chan==1&&effect.band==0);
        im(hrightmenu,"--");
        im(hrightmenu,"Left: 6 treble",rmFreqLeft+5, effect.chan==0&&effect.band==5);
        im(hrightmenu,"Left: 5",rmFreqLeft+4, effect.chan==0&&effect.band==4);
        im(hrightmenu,"Left: 4",rmFreqLeft+3, effect.chan==0&&effect.band==3);
        im(hrightmenu,"Left: 3",rmFreqLeft+2, effect.chan==0&&effect.band==2);
        im(hrightmenu,"Left: 2",rmFreqLeft+1, effect.chan==0&&effect.band==1);
        im(hrightmenu,"Left: 1 bass",rmFreqLeft+0, effect.chan==0&&effect.band==0);
      }
      else
      { stk::TColor &col = effect.cols[sel];
        string bitmapsel; if (col.type==ctBitmap) bitmapsel=col.bitmap;
        im(hrightmenu,"Bitmaps...",rmMoreBitmaps);
        for (int bic=0; bic<(int)body->bmps.size(); bic++)
        { string bname = body->bmps[bic].name;
          bool checked = (StringLower(bname)==StringLower(bitmapsel));
          im(hrightmenu,bname,rmBitmaps+bic,checked,true,true);
        }
        im(hrightmenu,"--");
        im(hrightmenu,"Colours...",rmColMore);
        list<COLORREF> cols; if (col.type==ctRGB) cols.push_back(CREF(col.rgb));
        im(hrightmenu,cols,col.type!=ctRGB,rmCol,false,&extracols);
        im(hrightmenu,"Fade",rmColFade,col.type==ctRGB && col.fadetonext,true,false);
        im(hrightmenu,"--");
        im(hrightmenu,"Invisible",rmInvisible,col.type==ctNone,true,false);
        im(hrightmenu,"--");
        if (effect.cols.size()>2) im(hrightmenu,"Delete",rmDelete);
        im(hrightmenu,"Add After",rmAddAfter);
        im(hrightmenu,"Add Before",rmAddBefore);
      }
      POINT pt; pt.x=x; pt.y=y; ClientToScreen(hwnd,&pt);
      BalanceMenu(hrightmenu);
      int cmd=TrackPopupMenu(hrightmenu,TPM_LEFTALIGN|TPM_TOPALIGN|TPM_RETURNCMD,pt.x,pt.y,0,hwnd,NULL);
      DestroyMenu(hrightmenu);
      if (cmd==rmFreqCumMore) editor->UserCumulatives(hwnd);
      else if (cmd>=rmFreqCumFirst && cmd<=rmFreqCumLast)
      { double d=retcums[cmd-rmFreqCumFirst].d;
        int r=retcums[cmd-rmFreqCumFirst].r;
        if (d==0) effect.cumulative=false;
        else {effect.cumulative=true; effect.cumrate=d; effect.creflect=r;}
      }
      else if (cmd==rmFreqNegative) {effect.negative=!effect.negative;}
      else if (cmd>=rmFreqDifference && cmd<rmFreqDifference+6) {effect.chan=2; effect.band=cmd-rmFreqDifference;}
      else if (cmd>=rmFreqRight && cmd<rmFreqRight+6) {effect.chan=1; effect.band=cmd-rmFreqRight;}
      else if (cmd>=rmFreqLeft && cmd<rmFreqLeft+6) {effect.chan=0; effect.band=cmd-rmFreqLeft;}
      else if (cmd>=rmFreqKaraoke && cmd<rmFreqKaraoke+6) {effect.chan=3; effect.band=cmd-rmFreqKaraoke;}
      else if (cmd==rmMoreBitmaps) editor->Bitmaps(GetParent(hwnd));
      else if (cmd>=rmBitmaps && cmd<rmBitmaps+50) {effect.cols[sel].type=ctBitmap; effect.cols[sel].bitmap=body->bmps[cmd-rmBitmaps].name; effect.cols[sel].bindex=-1;}
      else if (cmd==rmColMore) editor->UserColours(GetParent(hwnd));
      else if (cmd==rmColFade) {effect.cols[sel].type=ctRGB; effect.cols[sel].fadetonext=!effect.cols[sel].fadetonext;}
      else if (cmd>=rmCol && cmd<rmCol+50)
      { int r,g,b, c=cmd-rmCol;
        if (c<ncols) {r=defcols[c].r; g=defcols[c].g; b=defcols[c].b;}
        else if (c<ncols+(int)usercols.size())
        { int ci=ncols; list<TUserColour>::const_iterator ui=usercols.begin();
          while (ci<c) {ci++; ui++;}
          r=ui->c.r; g=ui->c.g; b=ui->c.b;
        }
        else
        { int e=c-ncols-(int)usercols.size(); COLORREF cl=extracols[e];
          r=GetRValue(cl); g=GetGValue(cl); b=GetBValue(cl);
        }
        effect.cols[sel].type=ctRGB; effect.cols[sel].rgb=RGBCOLOR(r,g,b);
      }
      else if (cmd==rmInvisible)
      { effect.cols[sel].type=ctNone;
      }
      else if (cmd==rmDelete)
      { double f=effect.fracs[sel];
        if (sel==0) effect.fracs[sel+1]+=f; else effect.fracs[sel-1]+=f;
        vector<stk::TColor>::iterator ci=effect.cols.begin()+sel; effect.cols.erase(ci);
        vector<double>::iterator fi=effect.fracs.begin()+sel; effect.fracs.erase(fi);
      }
      else if (cmd==rmAddBefore || cmd==rmAddAfter)
      { stk::TColor c; c.type=ctRGB; c.rgb=RGBCOLOR(rand()%255,rand()%255,rand()%255); c.fadetonext=true;
        double f=effect.fracs[sel]/2;
        effect.fracs[sel]=f;
        if (cmd==rmAddBefore) {effect.cols.insert(effect.cols.begin()+sel,c); effect.fracs.insert(effect.fracs.begin()+sel,f);}
        else if (sel<(int)effect.cols.size()-1) {effect.cols.insert(effect.cols.begin()+sel+1,c); effect.fracs.insert(effect.fracs.begin()+sel+1,f);}
        else {effect.cols.push_back(c); effect.fracs.push_back(f);}
      }
      InvalidateRect(hwnd,NULL,TRUE);
    } return 0;
    case WM_MOUSEMOVE:
    { if (dat->dragging==-2) return TRUE;
      int x=LOWORD(lParam);
      double f = ((double)(x-dat->rc.left))/((double)(dat->rc.right-dat->rc.left));
      if (f<0) f=0; if (f>1) f=1;
      if (dat->dragging==-1) {dat->knobx=dat->rc.left+(int)(f*(double)(dat->rc.right-dat->rc.left)); effect.f=f; InvalidateRect(hwnd,NULL,TRUE); return TRUE;}
      double tot=0; for (int segment=0; segment<dat->dragging; segment++) tot+=effect.fracs[segment];
      if (f-tot<0.01) return TRUE;
      double nextf=tot+effect.fracs[dat->dragging]+effect.fracs[dat->dragging+1];
      if (dat->dragging==(int)effect.cols.size()-2) nextf=1;
      if (nextf-f<0.01) return TRUE;
      effect.fracs[dat->dragging]=f-tot; effect.fracs[dat->dragging+1]=nextf-f;
      InvalidateRect(hwnd,NULL,TRUE);
    } return TRUE;
    case WM_LBUTTONUP:
    { dat->dragging=-2;
    } return TRUE;
  }
  return DefWindowProc(hwnd,msg,wParam,lParam);
}


string TEditor::EffectClick(HWND hpar,const string oldname)
{ //
  enum {smDelete=1, smRename=2, smAdd=3, smEdit=4};
  HMENU heffectmenu=CreatePopupMenu();
  // effects
  im(heffectmenu,"Add",smAdd);
  if (oldname!="")
  { im(heffectmenu,"Delete",smDelete);
    im(heffectmenu,"Rename",smRename);
    im(heffectmenu,"Edit",smEdit);
  }
  //
  POINT pt; GetCursorPos(&pt);
  int cmd=TrackPopupMenu(heffectmenu,TPM_LEFTALIGN|TPM_TOPALIGN|TPM_RETURNCMD,pt.x,pt.y,0,hpar,NULL);
  DestroyMenu(heffectmenu);
  if (cmd==0) return oldname;
  //
  MarkUndo();
  ismodified=true;
  if (cmd==smAdd)
  { string name=CleanEffectName("NewEffect");
    TEffect e; e.name=name; e.chan=nextchan; e.band=nextband;
    e.negative=false; e.cumulative=false; e.cumrate=1; e.f=0.3;
    if (e.chan>2) e.chan=0; // we don't want a special channel. just left/right/diff.
    stk::TColor c; c.type=ctRGB; c.rgb=RGBCOLOR(rand()%255,rand()%255,rand()%255); c.fadetonext=true; e.cols.push_back(c); e.fracs.push_back(0.4);
    c.rgb=RGBCOLOR(rand()%255,rand()%255,rand()%255); e.cols.push_back(c); e.fracs.push_back(0.6);
    body->effects.push_back(e);
    return name;
  }
  // otherwise, manipulate an existing one
  vector<TEffect>::iterator effectiterator=get_effect(body->effects,oldname);
  if (effectiterator==body->effects.end()) return oldname;
  //
  if (cmd==smEdit)
  { return "*"+oldname;
  }
  else if (cmd==smRename || cmd==smDelete)
  { string lcoldname=StringLower(oldname), newname="";
    if (cmd==smRename)
    { char c[100]; strcpy(c,oldname.c_str());
      int res = DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_RENAME),hpar,RenameDlgProc,(LPARAM)c);
      if (res!=IDOK || oldname==c) return oldname;
      newname=CleanEffectName(c);
    }
    // remove this effect from any limb/shapebrush/shapeline/style that refers to it:
    for (int i=0; i<body->nlimbs; i++)
    { if (StringLower(body->limbs[i].color.effect)==lcoldname)
      { body->limbs[i].linestyle=newname;
        if (cmd==smDelete && body->limbs[i].type==ctEffect) body->limbs[i].type=ctNone;
      }
    }
    for (vector<stk::TShape>::iterator si=body->shapes.begin(); si!=body->shapes.end(); si++)
    { if (StringLower(si->brush.effect)==lcoldname)
      { si->brush.effect=newname; 
        if (cmd==smDelete && si->brush.type==ctEffect) si->brush.type=ctNone;
      }
      if (StringLower(si->line.effect)==lcoldname)
      { si->line.effect=newname;
        if (cmd==smDelete && si->line.type==ctEffect) si->line.type=ctNone;
      }
    }
    for (list<TStyle>::iterator si=body->styles.begin(); si!=body->styles.end(); si++)
    { if (StringLower(si->limb.color.effect)==lcoldname)
      { si->limb.color.effect=newname;
        if (cmd==smDelete && si->limb.color.type==ctEffect) si->limb.color.type=ctNone;
      }
      if (StringLower(si->shape.brush.effect)==lcoldname)
      { si->shape.brush.effect=newname;
        if (cmd==smDelete && si->shape.brush.type==ctEffect) si->shape.brush.type=ctNone;
      }
    }
    //
    effectiterator->name=newname; if (cmd==smDelete) body->effects.erase(effectiterator);
    return newname;
  }
  //
  return oldname;
}


string TEditor::CleanEffectName(const string s)
{ string broot=s;
  for (int i=0; i<(int)broot.size(); i++)
  { if (broot[i]==')') broot[i]=']';
    if (broot[i]=='(') broot[i]='[';
    if (broot[i]=='=') broot[i]='-';
  }
  for (int i=1; ; i++)
  { string bname;
    if (i==1) bname=broot; else bname=broot+StringInt(i);
    bool okay=true;
    for (vector<TEffect>::const_iterator si=body->effects.begin(); si!=body->effects.end() && okay; si++)
    { if (StringLower(si->name)==StringLower(bname)) okay=false;
    }
    if (okay) return bname;
  }
}



int WINAPI EffectsDlgProc(HWND hdlg,UINT msg,WPARAM wParam,LPARAM lParam)
{ EffectsDlgData *dat;
  #pragma warning( push ) 
  #pragma warning( disable : 4244 4312 )
  if (msg==WM_INITDIALOG) {SetWindowLongPtr(hdlg,DWLP_USER,lParam); dat=(EffectsDlgData*)lParam;}
  else {dat=(EffectsDlgData*)GetWindowLongPtr(hdlg,DWLP_USER);}
  #pragma warning( pop )
  if (dat==NULL) return FALSE;
  TEditor *editor = dat->editor;
  TBody *body = editor->body;
  //
  switch (msg)
  { case WM_INITDIALOG:
    { // subclass the listbox
      HWND hlb = GetDlgItem(hdlg,IDC_LIST);
      LONG_PTR oldproc = GetWindowLongPtr(hlb,GWLP_WNDPROC);
      SetWindowLongPtr(hdlg,GWLP_USERDATA,oldproc);
      SetWindowLongPtr(hlb,GWLP_WNDPROC,(LONG_PTR)ListSubclassProc);
      SendMessage(hdlg,WM_APP,0,0);
      dat->timeStart=GetTickCount(); dat->timePrev=dat->timeStart; dat->samplei=rand()%3;
      SetTimer(hdlg,1,100,NULL);
    } return TRUE;
    case WM_APP: // reset the contents of the list box. lParam is the one to highlight
    { const char *csel = (const char*)lParam;
      SendDlgItemMessage(hdlg,IDC_LIST,LB_RESETCONTENT,0,0);
      for (vector<TEffect>::const_iterator si=body->effects.begin(); si!=body->effects.end(); si++)
      { string s = si->tostring();
        SendDlgItemMessage(hdlg,IDC_LIST,LB_ADDSTRING,0,(LPARAM)s.c_str());
      }
      if (body->effects.size()>0)
      { int seli=0;
        if (csel!=0) seli=SendDlgItemMessage(hdlg,IDC_LIST,LB_FINDSTRING,(WPARAM)-1,(LPARAM)(string(csel)+"=").c_str());
        SendDlgItemMessage(hdlg,IDC_LIST,LB_SETCURSEL,seli,0);
      }
    } return TRUE;
    case WM_TIMER:
    { if (dat->editup) return TRUE;
      int seli=SendDlgItemMessage(hdlg,IDC_LIST,LB_GETCURSEL,0,0); if (seli==LB_ERR) return TRUE;
      int len = SendDlgItemMessage(hdlg,IDC_LIST,LB_GETTEXTLEN,seli,0);
      char *c=new char[len+1]; SendDlgItemMessage(hdlg,IDC_LIST,LB_GETTEXT,seli,(LPARAM)c);
      char *eq=strchr(c,'='); if (eq!=0) *eq=0;
      vector<TEffect>::iterator ei=get_effect(body->effects,c); delete[] c;
      if (ei==body->effects.end()) return TRUE;
      TEffect &effect = *ei;
      //
      unsigned int nowt=GetTickCount();
      unsigned int time=nowt-dat->timeStart;
      unsigned int diff=nowt-dat->timePrev; dat->timePrev=nowt;
      double cmul = ((double)diff)/10.0;
      //
      unsigned int off = (time/41)%samplesize[dat->samplei];
      unsigned char *sd = sampledat[dat->samplei]+off*12;
      unsigned char s = sd[effect.chan*6+effect.band];
      double f = ((double)s)/255.0;
      if (effect.chan==2) f-=0.5; // difference channel, has bias
      if (effect.cumulative)
      { int dir=1; if (effect.negative) dir*=-1; if (effect.creflect<0) dir*=-1;
        double cf=effect.cumrate*f*cmul; if (effect.cumrate<0) cf=-effect.cumrate*cmul;
        effect.f += dir*0.01*cf;
        if (effect.creflect==0)
        { while (effect.f<0) effect.f+=1; while (effect.f>1) effect.f-=1;
        }
        else
        { for (;;)
          { if (effect.f>1) {effect.f=2-effect.f; effect.creflect*=-1;}
            else if (effect.f<0) {effect.f=-effect.f; effect.creflect*=-1;}
            else break;
          }
        }
      }
      else
      { if (effect.negative) effect.f=1-f;
        else effect.f=f;
      }
      //
      double tot=0; unsigned int segment;
      for (segment=0; segment<effect.cols.size()-1; segment++)
      { if (effect.f<tot+effect.fracs[segment]) break;
        tot+=effect.fracs[segment];
      }
      double nexttot=tot+effect.fracs[segment]; if (segment==effect.cols.size()-1) nexttot=1;
      stk::TColor col = effect.cols[segment];
      stk::TColor nextcol = effect.cols[(segment+1)%effect.cols.size()];
      //
      RECT rc; GetWindowRect(GetDlgItem(hdlg,IDC_EFFECT),&rc);
      ScreenToClient(hdlg,(POINT*)&rc.left); ScreenToClient(hdlg,(POINT*)&rc.right);
      HDC hdc=GetDC(hdlg);
      if (col.type==ctNone || col.type==ctBitmap) FillRect(hdc,&rc,(HBRUSH)GetStockObject(BLACK_BRUSH));
      else if (col.type==ctRGB)
      { LOGBRUSH lbr; lbr.lbStyle=BS_SOLID; lbr.lbColor=RGB(col.rgb.r,col.rgb.g,col.rgb.b);
        if (col.fadetonext && nextcol.type==ctRGB)
        { double r1=col.rgb.r, g1=col.rgb.g, b1=col.rgb.b;
          double r2=nextcol.rgb.r, g2=nextcol.rgb.g, b2=nextcol.rgb.b;
          double f=(effect.f-tot)/(nexttot-tot);
          double r=r1*(1-f)+r2*f, g=g1*(1-f)+g2*f, b=b1*(1-f)+b2*f;
          lbr.lbColor=RGB((int)r,(int)g,(int)b);
        }
        HBRUSH hbr=CreateBrushIndirect(&lbr);
        FillRect(hdc,&rc,hbr);
        DeleteObject(hbr);
      }
      else LUASSERTMM("unknown col type");
      if (col.type==ctBitmap)
      { HDC tdc = CreateCompatibleDC(hdc);
        const TBmp &bitmap = body->bmps[col.bindex];
        BITMAP bmp; GetObject(bitmap.hbm,sizeof(bmp),&bmp); int bwidth=bmp.bmWidth, bheight=bmp.bmHeight;
        SetStretchBltMode(hdc,COLORONCOLOR);
        int x,y,w,h;
        double fb=((double)bwidth)/((double)bheight), fr=((double)(rc.right-rc.left))/((double)(rc.bottom-rc.top));
        if (fb>fr)
        { x=rc.left; w=rc.right-rc.left;
          double f = ((double)w)/((double)bwidth);
          h=(int)(bheight*f);
          y=rc.top + (rc.bottom-rc.top-h)/2;
        }
        else
        { y=rc.top; h=rc.bottom-rc.top;
          double f = ((double)h)/((double)bheight);
          w=(int)(bwidth*f);
          x=rc.left + (rc.right-rc.left-w)/2;
        }
        // do it with the mask
        HGDIOBJ hold;
        if (bitmap.hbmask!=0)
        { hold = SelectObject(tdc,bitmap.hbmask);
          StretchBlt(hdc,x,y,w,h,tdc,0,0,bwidth,bheight,SRCAND);
          SelectObject(tdc,bitmap.hbm);
          StretchBlt(hdc,x,y,w,h,tdc,0,0,bwidth,bheight,SRCPAINT);
        }
        else
        { hold = SelectObject(tdc,bitmap.hbm);
          StretchBlt(hdc,x,y,w,h,tdc,0,0,bwidth,bheight,SRCCOPY);
        }
        SelectObject(tdc,hold);
        DeleteDC(tdc);
      }
      double dx=effect.f*(double)(rc.right-rc.left-8);
      int x=rc.left+(int)dx;
      rc.left=x-4; rc.right=x+4; rc.top=rc.bottom-8;
      FillRect(hdc,&rc,(HBRUSH)GetStockObject(WHITE_BRUSH));
      rc.left=x-2; rc.right=x+2; rc.top+=2; rc.bottom-=2;
      FillRect(hdc,&rc,(HBRUSH)GetStockObject(BLACK_BRUSH));
      ReleaseDC(hdlg,hdc);
    } return TRUE; 
    case WM_COMMAND:
    { int id=LOWORD(wParam), code=HIWORD(wParam);
      if (id==IDCANCEL) EndDialog(hdlg,id);
      else if (id==IDOK)
      { if (dat->editup)
        { ShowWindow(GetDlgItem(hdlg,IDC_LIST),SW_SHOW);
          ShowWindow(GetDlgItem(hdlg,IDC_LIST1),SW_SHOW);
          ShowWindow(GetDlgItem(hdlg,IDC_LIST2),SW_SHOW);
          ShowWindow(GetDlgItem(hdlg,IDC_EDIT1),SW_HIDE);
          ShowWindow(GetDlgItem(hdlg,IDC_EDIT2),SW_HIDE);
          SetWindowText(GetDlgItem(hdlg,IDOK),"Close");
          HWND hc = FindWindowEx(hdlg,0,"EffectsClass",NULL);
          if (hc!=0) SendMessage(hc,WM_COMMAND,IDOK,0);
          string newdef=dat->seffect; int pos=newdef.find("=");
          string newname; if (pos!=-1) newname=newdef.substr(0,pos); 
          vector<TEffect>::iterator ei=get_effect(body->effects,newname);
          if (ei!=body->effects.end()) ei->fromstring(newdef);
          SendMessage(hdlg,WM_APP,0,(LPARAM)newname.c_str());
          MakeBindexes(body); MakeEindexes(body);
          editor->Recalc(0);
          dat->editup=false;
        }
        else EndDialog(hdlg,id);
      }
      else if (id==IDC_LIST && code==BN_CLICKED)
      { int seli = SendDlgItemMessage(hdlg,IDC_LIST,LB_GETCURSEL,0,0);
        string oldname="", olddef="";
        if (seli!=LB_ERR)
        { int len = SendDlgItemMessage(hdlg,IDC_LIST,LB_GETTEXTLEN,seli,0);
          char *c=new char[len+1]; SendDlgItemMessage(hdlg,IDC_LIST,LB_GETTEXT,seli,(LPARAM)c);
          dat->seffect=c; char *eq=strchr(c,'='); if (eq!=0) *eq=0;
          oldname=c; delete[] c;
        }
        const string newname = editor->EffectClick(hdlg,oldname);
        bool edit=false;
        if (newname.length()>0 && newname[0]=='*') edit=true;
        if (!edit) {SendMessage(hdlg,WM_APP,0,(LPARAM)newname.c_str()); return TRUE;}
        dat->editup=true;
        WNDCLASSEX wcex; ZeroMemory(&wcex,sizeof(wcex)); wcex.cbSize = sizeof(WNDCLASSEX); 
        BOOL res=GetClassInfoEx(hInstance,"EffectsClass",&wcex);
        if (!res)
        { wcex.style = CS_HREDRAW | CS_VREDRAW;
          wcex.lpfnWndProc = (WNDPROC)EffectWndProc;
          wcex.hInstance = hInstance;
          wcex.hCursor = LoadCursor(NULL, IDC_ARROW);
          wcex.hbrBackground = (HBRUSH)GetStockObject(BLACK_BRUSH);
          wcex.lpszClassName = "EffectsClass";
          ATOM res=RegisterClassEx(&wcex);
          LUASSERT(res!=0);
        }
        RECT rc; GetWindowRect(GetDlgItem(hdlg,IDC_LIST),&rc); ScreenToClient(hdlg,(POINT*)&rc.left); int x=rc.left,y=rc.top;
        GetWindowRect(GetDlgItem(hdlg,IDC_EFFECT),&rc); ScreenToClient(hdlg,(POINT*)&rc.right); int w=rc.right-x,h=rc.bottom-y;
        CreateWindow("EffectsClass","",WS_CHILD|WS_VISIBLE,x,y,w,h,hdlg,0,hInstance,dat);
        ShowWindow(GetDlgItem(hdlg,IDC_LIST),SW_HIDE);
        ShowWindow(GetDlgItem(hdlg,IDC_LIST1),SW_HIDE);
        ShowWindow(GetDlgItem(hdlg,IDC_LIST2),SW_HIDE);
        ShowWindow(GetDlgItem(hdlg,IDC_EDIT1),SW_SHOW);
        ShowWindow(GetDlgItem(hdlg,IDC_EDIT2),SW_SHOW);
        SetWindowText(GetDlgItem(hdlg,IDOK),"OK");
      }
    } return TRUE;
    case WM_DESTROY:
    { KillTimer(hdlg,1);
    } return TRUE;
  }
  return FALSE;
}


void TEditor::Effects(HWND hpar)
{ EffectsDlgData dat;
  dat.editor=this;
  DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_EFFECTS),hpar,EffectsDlgProc,(LPARAM)&dat);
}


void TEditor::Category()
{ char c[MAX_PATH]; strcpy(c,body->category.c_str());
  int res = DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_RENAME),mhwnd,RenameDlgProc,(LPARAM)c);
  if (res!=IDOK) return;
  string newcat = string(c);
  if (newcat==body->category) return;
  MarkUndo();
  body->category = newcat;
  ismodified=true;
}

void TEditor::TargetFramerate()
{ char c[MAX_PATH]; wsprintf(c,"%i",body->fps);
  int res = DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_RENAME),mhwnd,RenameDlgProc,(LPARAM)c);
  if (res!=IDOK) return;
  int newfps; res=sscanf(c,"%i",&newfps);
  if (res!=1 || newfps<1) {MessageBeep(0); return;}
  if (newfps==body->fps) return;
  MarkUndo();
  body->fps = newfps;
  ismodified=true;
}

// ------------------------------------------------------------------------------------------
// ------------------------------------------------------------------------------------------
// ------------------------------------------------------------------------------------------

bool FileExists(const string fn)
{ HANDLE hFile=CreateFile(fn.c_str(),0,FILE_SHARE_READ|FILE_SHARE_WRITE,NULL,OPEN_EXISTING,0,NULL);
  if (hFile==INVALID_HANDLE_VALUE) return FALSE;
  CloseHandle(hFile); return TRUE;
}




bool AreFilenamesSame(const string fa,const string fb)
{ if (StringLower(fa)==StringLower(fb)) return true;
  HANDLE hfa = CreateFile(fa.c_str(),0,FILE_SHARE_READ|FILE_SHARE_WRITE,NULL,OPEN_EXISTING,0,NULL);
  HANDLE hfb = CreateFile(fb.c_str(),0,FILE_SHARE_READ|FILE_SHARE_WRITE,NULL,OPEN_EXISTING,0,NULL);
  BY_HANDLE_FILE_INFORMATION fia,fib;
  GetFileInformationByHandle(hfa,&fia);
  GetFileInformationByHandle(hfb,&fib);
  if (hfa!=INVALID_HANDLE_VALUE) CloseHandle(hfa);
  if (hfb!=INVALID_HANDLE_VALUE) CloseHandle(hfb);
  if (hfa==INVALID_HANDLE_VALUE || hfb==INVALID_HANDLE_VALUE) return false;
  bool same = (fia.nFileIndexHigh==fib.nFileIndexHigh);
  same &=     (fia.nFileIndexLow ==fib.nFileIndexLow);
  return same;
}

bool IsFileWithinDirectory(const string afn,const string adir)
{ string fn=StringLower(afn), dir=StringLower(adir);
  if (fn.find(dir)==0) return true;
  // maybe I should do something more sophisticated in case fn is a \\\network path
  // but dir is just a c:\normal\path,
  // or in case one of them is a short filename and the other is not.
  return false;
  // ... but I can't be bothered
}




void SelectAsDefault(const string fn)
{ vector<TPre> pre; DirScan(pre,GetStickDir()+"\\");
  string s = StringLower(ExtractFileName(fn));
  int preset=-1;
  for (int i=0; i<(int)pre.size(); i++)
  { string ps = StringLower(pre[i].path);
    if (ps.find(s)!=string::npos) preset=i+1;
  }
  if (preset==-1) return;
  HKEY hkey; DWORD disp; LONG res = RegCreateKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,NULL,0,KEY_WRITE,NULL,&hkey,&disp);
  if (res!=ERROR_SUCCESS) return;
  char pr[200]; wsprintf(pr,"%i",(int)preset);
  RegSetValueEx(hkey,"preset",0,REG_SZ,(LPBYTE)pr,(DWORD)strlen(pr)+1);
  RegCloseKey(hkey);
}

/*
// We used to remove duplicates. But this is a bad place to do it.
//
void FixUpRedundancy(HWND hwnd)
{ // first, check whether it's already been fixed up
  bool dealtwith=false;
  HKEY key; LONG res=RegOpenKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,KEY_READ,&key);
  if (res==ERROR_SUCCESS)
  { DWORD dat, type, size=sizeof(dat);
    res=RegQueryValueEx(key,"DealtWithRedundancy3.4",NULL,&type,(LPBYTE)&dat,&size);
    if (res==ERROR_SUCCESS) dealtwith=true;
    RegCloseKey(key);
  }
  if (dealtwith) return;
  // did we encounter any redundancy?
  vector<TPre> pre; DirScan(pre,GetStickDir()+"\\");
  const char *oss[] = {"amb bars","demented tweety-pie","droopy nose thing","jumping numbers",
    "fireworks (robert henderson)","jumping numbers","pac man","sound echoes",
    "whirls","windows","belly dancer","breakdancing lobster","dancer (vale)",
    "dancing stick guy (justin)","disco (mwstumpy)","girly goth dancer (midnight)",
    "riverdancer (midnight)","walk like an egyptian","bot head full (heppler)",
    "genie (evil boris)","johnny5 (evil boris)","lamp","snowman (jkabodnar)","spider (vale)",
    "band (marius kolesky)","bunny love (heppler)","bunny rock (manifest deity)",
    "drum (marius kolesky)","drummer2 (dod)","drummer (sneaky sneaks)","evil guitarist (lethal hairdo)",
    "rock guitarist (lim sh)","rocker (evil boris)","alberto (rk-pace)"};
  int nos=sizeof(oss)/sizeof(char*);
  vector<int> olds,news; olds.resize(nos); news.resize(nos); for (int i=0; i<nos; i++) {olds[i]=-1; news[i]=-1;}
  bool anyredundancy=false;
  for (int i=0; i<(int)pre.size(); i++)
  { string s = StringLower(pre[i].desc);
    const char *entry = s.c_str();
    for (int j=0; j<nos; j++)
    { const char *c = strstr(entry,oss[j]);
      if (c==entry) olds[j]=i; else if (c!=0) news[j]=i;
      if (olds[j]!=-1 && news[j]!=-1) anyredundancy=true;
    }
  }
  // shall we remove them?
  if (anyredundancy)
  { int ires=MessageBox(hwnd,"Since upgrading, there are =a few duplicate stick figures.\r\n"
                   "Shall I tidy them up now?\r\n\r\n"
                   "(This won't take long, and will only happen once.)","Duplicate stick figures",MB_YESNOCANCEL);
    if (ires!=IDYES) return;
  }
  // we're here either because there was no redundany, or because there was and we've
  // decided to tidy it up. In either case, we can write to the log.
  DWORD disp; res=RegCreateKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,NULL,0,KEY_WRITE,NULL,&key,&disp);
  if (res==ERROR_SUCCESS)
  { DWORD dat=1; RegSetValueEx(key,"DealtWithRedundancy3.4",0,REG_DWORD,(LPBYTE)&dat,sizeof(dat));
    RegCloseKey(key);
  }
  if (!anyredundancy) return;
  //
  // Our tidying subsists in this: for any duplicates, we check that they've got
  // identical file sizes, and if so we move the old one into an 'old' directory.
  string savedir=GetStickDir();     // "c:\\program files\\stick figures\\sticks"
  savedir=ExtractFilePath(savedir); // "c:\\program files\\stick figures"
  savedir=savedir+"\\duplicate sticks (can be deleted)"; 
  bool donesavedir=false;
  //
  for (int i=0; i<nos; i++)
  { if (olds[i]!=-1 && news[i]!=-1)
    { int jo=olds[i], jn=news[i];
      string desc = pre[jo].desc;
      string ofn = pre[jo].path;
      string nfn = pre[jn].path;
      string sfn = savedir+"\\"+ExtractFileName(ofn);
      HANDLE hofn=CreateFile(ofn.c_str(),GENERIC_READ,FILE_SHARE_READ,NULL,OPEN_EXISTING,0,NULL);
      HANDLE hnfn=CreateFile(nfn.c_str(),GENERIC_READ,FILE_SHARE_READ,NULL,OPEN_EXISTING,0,NULL);
      DWORD sizeo=0,sizen=0;
      if (hofn!=INVALID_HANDLE_VALUE) sizeo=GetFileSize(hofn,NULL);
      if (hnfn!=INVALID_HANDLE_VALUE) sizen=GetFileSize(hnfn,NULL);
      if (hofn!=INVALID_HANDLE_VALUE) CloseHandle(hofn);
      if (hnfn!=INVALID_HANDLE_VALUE) CloseHandle(hnfn);
      bool aresame = (sizeo==sizen && sizeo!=0 && sizen!=0);
      if (aresame)
      { if (!donesavedir) CreateDirectory(savedir.c_str(),NULL); donesavedir=true;
        MoveFileEx(ofn.c_str(),sfn.c_str(),MOVEFILE_COPY_ALLOWED);
      }
    }
  }
}
*/


class TInstallItemInfo
{ public:
  bool operator<(const TInstallItemInfo &b) const {return _stricmp(desc.c_str(),b.desc.c_str())<0;}
  string category, name, desc, fn;
  string srcfn; HZIP srczip; int srczi;
};
  


class TInstallDlgData
{ public:
  TInstallDlgData(vector<TInstallItemInfo> *i) : items(i), b(0) {}
  ~TInstallDlgData() {if (b!=0) delete b; b=0; if (hbm!=0) DeleteObject(hbm); hbm=0;}
  vector<TInstallItemInfo> *items;
  TBody *b; 
  unsigned int timeStart,timePrev; int samplei;
  RECT prc; HBITMAP hbm;
};

  
LRESULT CALLBACK KeylessListProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ HWND hpar = GetParent(hwnd); if (hpar==0) return DefWindowProc(hwnd,msg,wParam,lParam);
  LONG_PTR oldproc = GetWindowLongPtr(hpar,GWLP_USERDATA); if (oldproc==0) return DefWindowProc(hwnd,msg,wParam,lParam);
  switch (msg)
  { case WM_GETDLGCODE:
    { LRESULT code = CallWindowProc((WNDPROC)oldproc,hwnd,msg,wParam,lParam);
      code &= ~(DLGC_WANTALLKEYS|DLGC_WANTCHARS|DLGC_WANTMESSAGE);
      return code;
    }
  }
  return CallWindowProc((WNDPROC)oldproc,hwnd,msg,wParam,lParam);
}


int WINAPI InstallDlgProc(HWND hdlg,UINT msg,WPARAM wParam,LPARAM lParam)
{ TInstallDlgData *idd = (TInstallDlgData*)GetWindowLongPtr(hdlg,DWLP_USER);
  switch (msg)
  { case WM_INITDIALOG:
    { idd = (TInstallDlgData*)lParam; SetWindowLongPtr(hdlg,DWLP_USER,(LONG_PTR)idd);
      idd->timeStart=GetTickCount(); idd->timePrev=idd->timeStart;
      idd->samplei=rand()%3;
      unsigned int maxtime = (samplesize[idd->samplei]/12)*40; idd->timeStart -= rand()%maxtime;
      RECT rc; GetWindowRect(hdlg,&rc);
      int w=rc.right-rc.left, h=rc.bottom-rc.top;
      int cx=GetSystemMetrics(SM_CXSCREEN)/2, cy=GetSystemMetrics(SM_CYSCREEN)/2;
      MoveWindow(hdlg,cx-w/2,cy-h/2,w,h,FALSE);
      GetWindowRect(GetDlgItem(hdlg,IDC_PREVIEW),&idd->prc);
      ScreenToClient(hdlg,(POINT*)&idd->prc.left); ScreenToClient(hdlg,(POINT*)&idd->prc.right);
      HDC hdc=GetDC(0); idd->hbm=CreateCompatibleBitmap(hdc,w,h); ReleaseDC(0,hdc);
      // build the list
      bool anyalready=false;
      SendDlgItemMessage(hdlg,IDC_LIST,LB_RESETCONTENT,0,0);
      for (vector<TInstallItemInfo>::const_iterator i=idd->items->begin(); i!=idd->items->end(); i++)
      { string s = i->desc;
        if (FileExists(i->fn)) {s+="  *"; anyalready=true;}
        SendDlgItemMessage(hdlg,IDC_LIST,LB_ADDSTRING,0,(LPARAM)s.c_str());
      }
      // set the text
      string s = "Do you wish to install ";
      if (idd->items->size()>1) s+="copies of these stick figures?"; else s+="a copy of this stick figure?";
      if (anyalready) {s+="\r\n* will overwrite existing version.";}
      SetDlgItemText(hdlg,IDC_INSTALLPROMPT,s.c_str());
      // subclass the listbox
      LONG_PTR oldproc = GetWindowLongPtr(GetDlgItem(hdlg,IDC_LIST),GWLP_WNDPROC);
      SetWindowLongPtr(hdlg,GWLP_USERDATA,oldproc);
      SetWindowLongPtr(GetDlgItem(hdlg,IDC_LIST),GWLP_WNDPROC,(LONG_PTR)KeylessListProc);
      // final touches
      SendDlgItemMessage(hdlg,IDC_LIST,LB_SETCURSEL,0,0);
      SendMessage(hdlg,WM_COMMAND,(WPARAM)(IDC_LIST|(LBN_SELCHANGE<<16)),0);
      SetTimer(hdlg,1,40,NULL);
      PostMessage(hdlg,WM_TIMER,1,0);
      return TRUE;
    }
    case WM_TIMER:
    { if (idd->b==0) return TRUE;
      unsigned int nowt=GetTickCount();
      unsigned int nextt=idd->timePrev+1000/idd->b->fps;
      if (nowt<nextt) return TRUE;
      //
      unsigned int time=nowt-idd->timeStart;
      double cmul = ((double)(nowt-idd->timePrev))/10.0;
      idd->timePrev=nowt;
      //
      unsigned int off = (time/41)%samplesize[idd->samplei];
      unsigned char *sd = sampledat[idd->samplei]+off*12;
      double freq[3][6];
      for (int chan=0; chan<2; chan++)
      { for (int band=0; band<6; band++)
        { unsigned char s = sd[chan*6+band];
          double d = ((double)s)/255.0;
          freq[chan][band]=d;
        }
      }
      freq[2][0]=freq[0][3]; freq[2][1]=freq[1][4];
      //
      idd->b->AssignFreq(freq,cmul);
      idd->b->RecalcEffects(true);
      idd->b->Recalc();
      InvalidateRect(hdlg,&idd->prc,FALSE);
      return TRUE;
    }
    case WM_PAINT:
    { HDC hdc=GetDC(hdlg);
      //
      RECT drc; drc.left=0; drc.top=0; drc.right=idd->prc.right-idd->prc.left; drc.bottom=idd->prc.bottom-idd->prc.top;
      HDC sdc=GetDC(0), tdc=CreateCompatibleDC(sdc); ReleaseDC(0,sdc);
      HGDIOBJ hold=SelectObject(tdc,idd->hbm);
      SimpleDraw(tdc,drc,idd->b);
      BitBlt(hdc,idd->prc.left,idd->prc.top,idd->prc.right-idd->prc.left,idd->prc.bottom-idd->prc.top,tdc,0,0,SRCCOPY);
      SelectObject(tdc,hold); DeleteDC(tdc);
      ReleaseDC(hdlg,hdc);
      return FALSE;
    }
    case WM_COMMAND:
    { int id=LOWORD(wParam), code=HIWORD(wParam);
      if (id==IDC_LIST && code==LBN_SELCHANGE)
      { // load up a stick
        int i=SendDlgItemMessage(hdlg,IDC_LIST,LB_GETCURSEL,0,0);
        if (i==LB_ERR) return FALSE;
        TInstallItemInfo &inf = idd->items->at(i);
        char err[1000];
        if (idd->b==0) idd->b=new TBody();
        if (inf.srcfn!="") LoadBody(&idd->b,inf.srcfn.c_str(),err,lbForUse);
        else LoadBodyZip(&idd->b,inf.srczip,inf.srczi,err,lbForUse);
        idd->timeStart=GetTickCount();
        idd->samplei=rand()%3;
        unsigned int maxtime = (samplesize[idd->samplei]/12)*40; idd->timeStart -= rand()%maxtime;
      }
      if ((id==IDYES || id==IDNO || id==IDCANCEL))
      { if (idd->b!=0) delete idd->b; idd->b=0;
        EndDialog(hdlg,id);
      }
      if (id==IDYES)
      { // copy it
        SetCapture(hdlg);
        SetCursor(LoadCursor(NULL,IDC_WAIT));
        string newdefault="";
        for (vector<TInstallItemInfo>::const_iterator i=idd->items->begin(); i!=idd->items->end(); i++)
        { const TInstallItemInfo &inf = *i;
          ForceDirectories(ExtractFilePath(inf.fn));
          if (inf.srcfn!="") CopyFile(inf.srcfn.c_str(),inf.fn.c_str(),FALSE);
          else UnzipItem(inf.srczip,inf.srczi, inf.fn.c_str());
          newdefault=i->fn;
        }
        SelectAsDefault(newdefault);
        SetCursor(LoadCursor(NULL,IDC_ARROW));
        ReleaseCapture();
      }
      if ((id==IDYES || id==IDNO))
      { // show the folder
        string stickdir=GetStickDir();
        ShellExecute(NULL,"open",stickdir.c_str(),NULL,NULL,SW_SHOWNORMAL);
      }
      return TRUE;
    }
    case WM_DESTROY:
    { KillTimer(hdlg,1);
      return TRUE;
    }
  }
  return FALSE;
}




string CleanWebEscapeChars(const string fn)
{ size_t len=fn.length();
  if (len==0) return "";
  // get rid of the %20 entries, replacing them with spaces
  string s(len,0); const char *c=fn.c_str();
  for (size_t i=0; i<len; i++)
  { if (c[0]=='%' && c[1]=='2' && c[2]=='0') {s[i]=' '; c+=3;}
    else {s[i]=*c; c++;}
  }
  // maybe s has the form filename[1].stk or filename[1], in which
  // case we remove the number and square brackets
  c=s.c_str();
  const char *close=strrchr(c,']'), *open=strrchr(c,'[');
  if (close!=0 && open!=0 && open<close && (close[1]=='.' || close[1]==0))
  { bool allnums=true;
    for (const char *d=open+1; d<close; d++)
    { if (*d<'0' || *d>'9') allnums=false;
    }
    if (allnums)
    { string t = string(c,open-c);
      if (close[1]!=0) t+=string(close+1);
      s=t;
    }
  }
  return s;
}



int InstallZipFile(const char *fn,bool *wasjustastick)
{ string stickdir=GetStickDir();
  HZIP hz = OpenZip(fn,NULL);
  ZIPENTRY ze; GetZipItem(hz,-1,&ze); int numentries=ze.index;
  vector<TInstallItemInfo> items;
  int numstk=0, numbmp=0, numstyletxt=0, numsticktxt=0, numother=0;
  for (int i=0; i<numentries; i++)
  { GetZipItem(hz,i,&ze);
    string fn(ze.name);
    if (StringLower(ze.name)=="mainstick.txt") {numsticktxt++; continue;}
    if (StringLower(ze.name)=="styles.txt") {numstyletxt++; continue;}
    if (StringLower(stk::ExtractFileExt(ze.name))==".bmp") {numbmp++; continue;}
    if (StringLower(stk::ExtractFileExt(ze.name))!=".stk") {numother++; continue;}
    numstk++;
    fn=CleanWebEscapeChars(stk::ChangeFileExt(stk::ExtractFileName(ze.name),""));
    char buf[1000]; UnzipItem(hz,i,buf,1000); buf[999]=0;
    // normally what we've unzipped will be a stick header. But we also
    // allow for badly-formed .stk files which lack the header and go
    // straight into a zip. (i.e. someone broke their zip-header).
    DWORD magic = * ((DWORD*)buf);
    bool iszipfile = (magic==0x04034b50);
    iszipfile |= (magic==0x5a4b5453);
    bool isstickfile = (magic==0x6d696c6e);
    if (iszipfile)
    { char *fullbuf = new char[ze.unc_size];
      UnzipItem(hz,i,fullbuf,ze.unc_size);
      // !!! seems to be a bug here. The file we unzipped was the correct size
      // but has the wrong data. Maybe a problem with unzipping a zip that contains a zip.
      HZIP hsubz = OpenZip(fullbuf,ze.unc_size,NULL);
      if (hsubz!=0)
      { int subi; FindZipItem(hsubz,"mainstick.txt",true,&subi,NULL);
        if (subi!=-1)
        { UnzipItem(hsubz,subi,buf,1000); buf[999]=0; isstickfile=true;
        }
        CloseZip(hsubz);
      }
    }
    if (!isstickfile) continue;
    const char *c=strstr(buf,"\ncategory="); string category("");
    if (c!=0) {c+=10; while (*c==' ') c++; if (*c==0) c=0;}
    if (c!=0)
    { const char *end=c; while (*end!='\r' && *end!='\n' && *end!=0) end++; 
      while (end>c+1 && end[-1]==' ') end--; category=string(c,end-c);
    }
    TInstallItemInfo iii;
    iii.category=category; iii.name=fn;
    if (category=="") iii.desc=fn; else iii.desc=category+"\\"+fn;
    iii.fn=stickdir+"\\"+iii.desc+".stk";
    iii.srcfn=""; iii.srczip=hz; iii.srczi=i;
    items.push_back(iii);
  }
  if (numstk==0 && numsticktxt>0) {CloseZip(hz); *wasjustastick=true; return IDCANCEL;}
  if (items.size()==0) {CloseZip(hz); MessageBox(NULL,"No stick figures found","Sticky",MB_OK); return IDCANCEL;}
  sort(items.begin(),items.end());
  TInstallDlgData idd(&items);
  int res = DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_INSTALL),NULL,InstallDlgProc,(LPARAM)&idd);
  CloseZip(hz);
  return res;
}


typedef struct {string artist, title, filename, regexp, stick;} TMatchData;

typedef struct
{ list<string> recents; vector<TMatchData> cur_recents;
  list<TRule> rules; vector<TMatchData> cur_rules;
  vector<TPre> *pre;
  string re; string stick; bool ignore;
} TAssocDlgData;

string reggarble(const string s,bool emptyiswildcard)
{ if (emptyiswildcard && s=="") return ".*";
  string t; size_t len=s.length();
  for (size_t i=0; i<len; i++)
  { if (s[i]=='*') t+=".*";
    else
    { if (strchr(".+@#()&$|[]{}\\",s[i])!=0) t+="\\";
      t+=string(&s[i],1);
    }
  }
  return t;
}

string regungarble(const string s)
{ string t; size_t len=s.length();
  for (size_t i=0; i<len; i++)
  { if (s[i]=='.' && i<len-1 && s[i+1]=='*') {i++; t+="*";}
    else
    { bool escape = (s[i]=='\\');
      if (escape) {if (i==len-1) return ""; else i++;}
      bool ctrl = (strchr(".*+@#()&$|[]{}\\",s[i])!=0);
      if (escape&&!ctrl) return ""; // regexp had a slash in a way that we didn't create, eg. \n
      if (!escape&&ctrl) return ""; // regexp had a ctrl char, which we don't understand
      t+=string(&s[i],1);
    }
  }
  return t;
}

string make_standard_re(const string artist, const string title, const string filename)
{ string re = "ARTIST="+reggarble(artist,true)+"___TITLE="+reggarble(title,true)+"___FILENAME="+reggarble(filename,true)+"___";
  return re;
}

bool get_standard_re(const string s, string &artist, string &title, string &filename)
{ // we wish to see whether the regexp is in a form we understand
  // i.e. ARTIST=pattern___TITLE=pattern___FILENAME=pattern___
  const char *c = s.c_str();
  REGEXP re; regbegin(&re,"ARTIST=(.*)___TITLE=(.*)___FILENAME=(.*)___",0);
  REGMATCH m[4]; bool match = regmatch(&re,c,m,4);
  regend(&re);
  if (!match) return false;
  artist = string(c+m[1].i,m[1].len); artist=regungarble(artist);
  title = string(c+m[2].i,m[2].len); title=regungarble(title);
  filename = string(c+m[3].i,m[3].len); filename=regungarble(filename);
  return (artist!="" && title!="" && filename!="");
}
        

     
LRESULT CALLBACK StickSubclassProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ HWND hpar = GetParent(hwnd); if (hpar==0) return DefWindowProc(hwnd,msg,wParam,lParam);
  LONG_PTR oldproc = GetWindowLongPtr(hpar,GWLP_USERDATA); if (oldproc==0) return DefWindowProc(hwnd,msg,wParam,lParam);
  if (msg==WM_RBUTTONDOWN)
  { SetFocus(hwnd);
    int id = GetWindowLongPtr(hwnd,GWLP_ID);
    SendMessage(hpar,WM_COMMAND,(WPARAM)(id|(BN_CLICKED<<16)),(LPARAM)hwnd);
    return 0;
  }
  else if (msg==WM_RBUTTONUP) return 0;
  return CallWindowProc((WNDPROC)oldproc,hwnd,msg,wParam,lParam);
}




int WINAPI RuleDlgProc(HWND hdlg,UINT msg,WPARAM wParam,LPARAM lParam)
{ TAssocDlgData *add = (TAssocDlgData*)GetWindowLongPtr(hdlg,DWLP_USER);
  switch (msg)
  { case WM_INITDIALOG:
    { add = (TAssocDlgData*)lParam; SetWindowLongPtr(hdlg,DWLP_USER,(LONG_PTR)add);
      add->ignore=false;
      SetDlgItemText(hdlg,IDC_REGEXP,add->re.c_str());
      SetDlgItemText(hdlg,IDC_STICKRE,add->stick.c_str());
      // subclass the stick edit box
      HWND hstick = GetDlgItem(hdlg,IDC_STICK);
      LONG_PTR oldproc = GetWindowLongPtr(hstick,GWLP_WNDPROC);
      SetWindowLongPtr(hdlg,GWLP_USERDATA,oldproc);
      SetWindowLongPtr(hstick,GWLP_WNDPROC,(LONG_PTR)StickSubclassProc);
      return TRUE;
    }
    case WM_APP+1: // deduce regexp from fields
    { char c[5000];
      GetDlgItemText(hdlg,IDC_ARTIST,c,5000); string artist(c);
      GetDlgItemText(hdlg,IDC_TITLE,c,5000); string title(c);
      GetDlgItemText(hdlg,IDC_FILENAME,c,5000); string filename(c);
      string re = make_standard_re(artist,title,filename);
      GetDlgItemText(hdlg,IDC_STICK,c,5000);
      string sre = reggarble(c);
      add->ignore=true;
      SetDlgItemText(hdlg,IDC_REGEXP,re.c_str());
      SetDlgItemText(hdlg,IDC_STICKRE,sre.c_str());
      add->ignore=false;
      return TRUE;
    }
    case WM_APP: // deduce fields from regexp
    { char c[5000]; GetDlgItemText(hdlg,IDC_REGEXP,c,5000);
      string artist, title, filename;
      bool stdform = get_standard_re(c,artist,title,filename);
      GetDlgItemText(hdlg,IDC_STICKRE,c,5000);
      string stick = regungarble(c);
      add->ignore=true;
      SetDlgItemText(hdlg,IDC_ARTIST,stdform?artist.c_str():"");
      SetDlgItemText(hdlg,IDC_TITLE,stdform?title.c_str():"");
      SetDlgItemText(hdlg,IDC_FILENAME,stdform?filename.c_str():"");
      SetDlgItemText(hdlg,IDC_STICK,stick.c_str());
      add->ignore=false;
      EnableWindow(GetDlgItem(hdlg,IDC_ARTIST),stdform);
      EnableWindow(GetDlgItem(hdlg,IDC_TITLE),stdform);
      EnableWindow(GetDlgItem(hdlg,IDC_FILENAME),stdform);
      EnableWindow(GetDlgItem(hdlg,IDC_STICK),stick!="");
      return TRUE;
    }
    case WM_COMMAND:
    { int id=LOWORD(wParam), code=HIWORD(wParam);
      if (!add->ignore && (id==IDC_STICK||id==IDC_ARTIST||id==IDC_TITLE||id==IDC_FILENAME)&&code==EN_CHANGE) SendMessage(hdlg,WM_APP+1,0,0);
      if (!add->ignore && (id==IDC_REGEXP||id==IDC_STICKRE) && code==EN_CHANGE) SendMessage(hdlg,WM_APP,0,0);
      if (id==IDC_HELPBTN && code==BN_CLICKED)
      { string hlp = GetStickDir()+"\\associate sticks with music.stam";
        string cmd = "notepad.exe \""+hlp+"\"";
        STARTUPINFO si; ZeroMemory(&si,sizeof(si)); si.cb=sizeof(si);
        PROCESS_INFORMATION pi; ZeroMemory(&pi,sizeof(pi));
        BOOL res = CreateProcess(NULL,(char*)cmd.c_str(),NULL,NULL,FALSE,0,NULL,NULL,&si,&pi);
        if (res) {CloseHandle(pi.hProcess); CloseHandle(pi.hThread);}
        else MessageBeep(0);
      }
      if (id==IDC_STICK && code==BN_CLICKED)
      { if (add->pre->size()==0)
        { SetCapture(hdlg); SetCursor(LoadCursor(NULL,IDC_WAIT));
          DirScan(*(add->pre),GetStickDir()+"\\",true);
          SetCursor(LoadCursor(NULL,IDC_ARROW)); ReleaseCapture();
        }
        HMENU hmenu=CreatePopupMenu();
        for (int i=add->pre->size()-1; i>=0; i--)
        { string s = add->pre->at(i).desc;
          PopulateMenu(hmenu,s.c_str(),1000+i,false);
        }
        POINT pt; GetCursorPos(&pt); RECT rc; GetWindowRect(GetDlgItem(hdlg,IDC_STICK),&rc);
        if (pt.x<rc.left) pt.x=rc.left; if (pt.x>rc.right) pt.x=rc.right;
        if (pt.y<rc.top) pt.y=rc.top; if (pt.y>rc.bottom) pt.y=rc.bottom;
        int cmd=TrackPopupMenu(hmenu,TPM_LEFTALIGN|TPM_TOPALIGN|TPM_RETURNCMD,pt.x,pt.y,0,hdlg,NULL);
        DestroyMenu(hmenu);
        if (cmd>=1000)
        { string stick = add->pre->at(cmd-1000).regexp;
          SetDlgItemText(hdlg,IDC_STICK,stick.c_str());
        }
      }
      if (id==IDOK)
      { char c[5000]; REGEXP re;
        GetDlgItemText(hdlg,IDC_REGEXP,c,5000); string match(c);
        GetDlgItemText(hdlg,IDC_STICKRE,c,5000); string stickre(c);
        bool matchok = regbegin(&re,match.c_str(),0); regend(&re);
        bool stickok = regbegin(&re,stickre.c_str(),0); regend(&re);
        if (!matchok || !stickok)
        { MessageBox(hdlg,"Not a valid regular-expression","Error",MB_OK);
          SendDlgItemMessage(hdlg,matchok?IDC_STICKRE:IDC_REGEXP,EM_SETSEL,0,(LPARAM)-1);
          SetFocus(GetDlgItem(hdlg,matchok?IDC_STICKRE:IDC_REGEXP));
        }
        else
        { add->re = match;
          add->stick = stickre;
          EndDialog(hdlg,IDOK);
        }
      }
      if (id==IDCANCEL) EndDialog(hdlg,IDCANCEL);
      return TRUE;
    }
  }
  return FALSE;
}



LRESULT CALLBACK DellistSubclassProc(HWND hwnd,UINT msg,WPARAM wParam,LPARAM lParam)
{ HWND hpar = GetParent(hwnd); if (hpar==0) return DefWindowProc(hwnd,msg,wParam,lParam);
  LONG_PTR oldproc = GetWindowLongPtr(hpar,GWLP_USERDATA); if (oldproc==0) return DefWindowProc(hwnd,msg,wParam,lParam);
  if (msg==WM_KEYDOWN && (wParam==VK_DELETE || wParam==VK_BACK))
  { SendMessage(hpar,WM_COMMAND,(WPARAM)(IDC_REMOVE|(BN_CLICKED<<16)),0);
  }
  return CallWindowProc((WNDPROC)oldproc,hwnd,msg,wParam,lParam);
}



int WINAPI AssocDlgProc(HWND hdlg,UINT msg,WPARAM wParam,LPARAM lParam)
{ TAssocDlgData *add = (TAssocDlgData*)GetWindowLongPtr(hdlg,DWLP_USER);
  switch (msg)
  { case WM_INITDIALOG:
    { add = (TAssocDlgData*)lParam; SetWindowLongPtr(hdlg,DWLP_USER,(LONG_PTR)add);
      RECT rc; GetWindowRect(hdlg,&rc); int w=rc.right-rc.left, h=rc.bottom-rc.top;
      int cx=GetSystemMetrics(SM_CXSCREEN)/2, cy=GetSystemMetrics(SM_CYSCREEN)/2;
      MoveWindow(hdlg,cx-w/2,cy-h/2,w,h,FALSE);
      SendMessage(hdlg,WM_APP,0,0); // recents
      SendMessage(hdlg,WM_APP+1,0,0); // rules
      SendDlgItemMessage(hdlg,IDC_RULES,LB_SETCURSEL,(WPARAM)-1,0);
      SendDlgItemMessage(hdlg,IDC_RECENTS,LB_SETCURSEL,(WPARAM)-1,0);
      SendMessage(hdlg,WM_COMMAND,(WPARAM)(IDC_RULES|(LBN_SELCHANGE<<16)),0); // enable buttons
      // subclass the rules listbox
      HWND hlb = GetDlgItem(hdlg,IDC_RULES);
      LONG_PTR oldproc = GetWindowLongPtr(hlb,GWLP_WNDPROC);
      SetWindowLongPtr(hdlg,GWLP_USERDATA,oldproc);
      SetWindowLongPtr(hlb,GWLP_WNDPROC,(LONG_PTR)DellistSubclassProc);
      return TRUE;
    }
    case WM_APP+1: // populate the "rules" box
    { HFONT hfont = (HFONT)SendDlgItemMessage(hdlg,IDC_RULES,WM_GETFONT,0,0);
      HDC hdc=GetDC(0); HGDIOBJ holdf=0; if (hfont!=0) holdf=SelectObject(hdc,hfont);
      int maxwidth=0;
      //
      SendDlgItemMessage(hdlg,IDC_RULES,WM_SETREDRAW,FALSE,0);
      SendDlgItemMessage(hdlg,IDC_RULES,LB_RESETCONTENT,0,0);
      add->cur_rules.clear();
      for (list<TRule>::const_iterator ir=add->rules.begin(); ir!=add->rules.end(); ir++)
      { string sif, sthen;
        TMatchData md; md.regexp=ir->regexp; md.stick=ir->stick;
        bool stdform = get_standard_re(md.regexp,md.artist,md.title,md.filename);
        if (stdform)
        { if (md.title!="*") sif+="title="+md.title;
          if (md.artist!="*") {if (sif!="") sif+=" and "; sif+="artist="+md.artist;}
          if (md.filename!="*") {if (sif!="") sif+=" and "; sif+="filename="+md.filename;}
        }
        else sif=ir->regexp;
        string stick = regungarble(md.stick);
        if (stick=="") sthen=md.stick; else sthen=stick;
        string s = "if "+sif+" then stick="+sthen;
        SendDlgItemMessage(hdlg,IDC_RULES,LB_ADDSTRING,0,(LPARAM)s.c_str());
        add->cur_rules.push_back(md);
        RECT rc={0}; DrawText(hdc,s.c_str(),-1,&rc,DT_CALCRECT);
        if (rc.right>maxwidth) maxwidth=rc.right;
      }
      SendDlgItemMessage(hdlg,IDC_RULES,LB_SETHORIZONTALEXTENT,maxwidth+8,0);
      SendDlgItemMessage(hdlg,IDC_RULES,WM_SETREDRAW,TRUE,0);
      InvalidateRect(GetDlgItem(hdlg,IDC_RULES),NULL,TRUE);
      if (holdf!=0) SelectObject(hdc,holdf); ReleaseDC(0,hdc);
      return TRUE;
    }
    case WM_APP: // populate the "recents" box. If wParam is true then we also update add->recents
    { HFONT hfont = (HFONT)SendDlgItemMessage(hdlg,IDC_RULES,WM_GETFONT,0,0);
      HDC hdc=GetDC(0); HGDIOBJ holdf=0; if (hfont!=0) holdf=SelectObject(hdc,hfont);
      int maxwidth=0;
      //
      list<string> newrecents;
      SendDlgItemMessage(hdlg,IDC_RECENTS,WM_SETREDRAW,FALSE,0);
      SendDlgItemMessage(hdlg,IDC_RECENTS,LB_RESETCONTENT,0,0);
      add->cur_recents.clear(); add->cur_recents.reserve(add->recents.size());
      REGEXP re; regbegin(&re,"ARTIST=(.*)___TITLE=(.*)___FILENAME=(.*)___STICK=(.*)___",0);
      // we'll pregenerate regexps for all the rules since otherwise this would
      // have a numrecents*numrules*costof(regcompile), which is too slow!
      int numrules = add->rules.size(); vector<REGEXP> rerules(numrules);
      int i=0; for (list<TRule>::const_iterator ir=add->rules.begin(); ir!=add->rules.end(); ir++,i++)
      { regbegin(&rerules[i],ir->regexp.c_str(),REG_ICASE);
      }
      //
      for (list<string>::const_iterator ir=add->recents.begin(); ir!=add->recents.end(); ir++)
      { REGMATCH m[5]; const char *src=ir->c_str();
        bool res = regmatch(&re,src,m,5);
        if (res)
        { TMatchData md;
          md.artist=string(src+m[1].i,m[1].len);
          md.title=string(src+m[2].i,m[2].len);
          md.filename=string(src+m[3].i,m[3].len);
          md.stick=string(src+m[4].i,m[4].len); if (md.stick=="") md.stick=".*";
          // replace ...Music\\stuff by just *\\stuff and .mp3/.wma by .*
          string matchfn=md.filename;
          size_t mpos = StringLower(matchfn).find("music\\");
          if (mpos!=string::npos) matchfn="*"+matchfn.substr(mpos+5);
          int len=matchfn.length();
          if (len>4 && StringLower(matchfn.substr(len-4))==".mp3") matchfn=matchfn.substr(0,len-4)+".*";
          if (len>4 && StringLower(matchfn.substr(len-4))==".wma") matchfn=matchfn.substr(0,len-4)+".*";
          //
          md.regexp="ARTIST="+reggarble(md.artist,true)+"___TITLE="+reggarble(md.title,true)+"___FILENAME="+reggarble(matchfn,true)+"___";
          string mus=make_music_string(md.artist,md.title,md.filename);
          bool match=false;
          for (int i=0; i<numrules && !match; i++) match|=regmatch(&rerules[i],mus.c_str(),NULL,0);
          if (!match)
          { newrecents.push_back(ir->c_str());
            string s=md.artist;
            if (s!="") s+=" -- "; s+=md.title;
            if (s!="") s+=" -- "; s+=md.filename;
            SendDlgItemMessage(hdlg,IDC_RECENTS,LB_ADDSTRING,0,(LPARAM)s.c_str());
            add->cur_recents.push_back(md);
            RECT rc={0}; DrawText(hdc,s.c_str(),-1,&rc,DT_CALCRECT);
            if (rc.right>maxwidth) maxwidth=rc.right;
          }
        }
      }
      regend(&re);
      for (int i=0; i<numrules; i++) regend(&rerules[i]);
      if (wParam==TRUE)
      { add->recents.clear(); add->recents.splice(add->recents.begin(),newrecents);
      }
      SendDlgItemMessage(hdlg,IDC_RECENTS,LB_SETHORIZONTALEXTENT,maxwidth+8,0);
      SendDlgItemMessage(hdlg,IDC_RECENTS,WM_SETREDRAW,TRUE,0);
      InvalidateRect(GetDlgItem(hdlg,IDC_RECENTS),NULL,TRUE);
      if (holdf!=0) SelectObject(hdc,holdf); ReleaseDC(0,hdc);
      return TRUE;
    }
    case WM_COMMAND:
    { int id=LOWORD(wParam), code=HIWORD(wParam);
      if (id==IDC_HELPBTN && code==BN_CLICKED)
      { string hlp = GetStickDir()+"\\associate sticks with music.stam";
        string cmd = "notepad.exe \""+hlp+"\"";
        STARTUPINFO si; ZeroMemory(&si,sizeof(si)); si.cb=sizeof(si);
        PROCESS_INFORMATION pi; ZeroMemory(&pi,sizeof(pi));
        BOOL res = CreateProcess(NULL,(char*)cmd.c_str(),NULL,NULL,FALSE,0,NULL,NULL,&si,&pi);
        if (res) {CloseHandle(pi.hProcess); CloseHandle(pi.hThread);}
        else MessageBeep(0);
      }
      if (id==IDC_RULES && code==LBN_SELCHANGE)
      { int i = SendDlgItemMessage(hdlg,IDC_RULES,LB_GETCURSEL,0,0);
        int count = SendDlgItemMessage(hdlg,IDC_RULES,LB_GETCOUNT,0,0);
        EnableWindow(GetDlgItem(hdlg,IDC_MODIFY), i!=LB_ERR);
        EnableWindow(GetDlgItem(hdlg,IDC_REMOVE), i!=LB_ERR);
        EnableWindow(GetDlgItem(hdlg,IDC_UP), i!=LB_ERR && i!=0);
        EnableWindow(GetDlgItem(hdlg,IDC_DOWN), i!=LB_ERR && i<count-1);
        SendDlgItemMessage(hdlg,IDC_RECENTS,LB_SETCURSEL,(WPARAM)-1,0);
      }
      if (id==IDC_RECENTS && code==LBN_SELCHANGE)
      { EnableWindow(GetDlgItem(hdlg,IDC_MODIFY), false);
        EnableWindow(GetDlgItem(hdlg,IDC_REMOVE), false);
        EnableWindow(GetDlgItem(hdlg,IDC_UP), false);
        EnableWindow(GetDlgItem(hdlg,IDC_DOWN), false);
        SendDlgItemMessage(hdlg,IDC_RULES,LB_SETCURSEL,(WPARAM)-1,0);
      }
      if ((id==IDC_UP||id==IDC_DOWN) && code==BN_CLICKED)
      { int selrule = SendDlgItemMessage(hdlg,IDC_RULES,LB_GETCURSEL,0,0);
        if (selrule!=LB_ERR)
        { list<TRule>::iterator ir=add->rules.begin();
          for (int i=0; i<selrule; i++) ir++;
          TRule r = *ir;
          ir=add->rules.erase(ir);
          if (id==IDC_UP) ir--; else ir++;
          add->rules.insert(ir,r);
          //
          SendMessage(hdlg,WM_APP+1,0,0); // update the rules list
          if (id==IDC_UP) selrule--; else selrule++;
          SendDlgItemMessage(hdlg,IDC_RULES,LB_SETCURSEL,selrule,0);
          SendMessage(hdlg,WM_COMMAND,(WPARAM)(IDC_RULES|(LBN_SELCHANGE<<16)),0);
        }
      }
      if (id==IDC_REMOVE && code==BN_CLICKED)
      { int selrule = SendDlgItemMessage(hdlg,IDC_RULES,LB_GETCURSEL,0,0);
        int count = SendDlgItemMessage(hdlg,IDC_RULES,LB_GETCOUNT,0,0);
        if (selrule!=LB_ERR)
        { list<TRule>::iterator ir=add->rules.begin();
          for (int i=0; i<selrule; i++) ir++;
          add->rules.erase(ir);
          SetCursor(LoadCursor(NULL,IDC_WAIT));
          SendMessage(hdlg,WM_APP,0,0);   // update the recents list
          SendMessage(hdlg,WM_APP+1,0,0); // update the rules list
          int newsel=selrule; if (newsel==count-1) newsel--; 
          SendDlgItemMessage(hdlg,IDC_RULES,LB_SETCURSEL,newsel,0);
          SendMessage(hdlg,WM_COMMAND,(WPARAM)(IDC_RULES|(LBN_SELCHANGE<<16)),0);
          SetCursor(LoadCursor(NULL,IDC_ARROW));
        }
      }
      if ((id==IDC_MODIFY && code==BN_CLICKED) || (id==IDC_RULES && code==LBN_DBLCLK))
      { TAssocDlgData subd; subd.pre=add->pre;
        int selrule = SendDlgItemMessage(hdlg,IDC_RULES,LB_GETCURSEL,0,0);
        if (selrule!=LB_ERR)
        { subd.re = add->cur_rules[selrule].regexp; subd.stick=add->cur_rules[selrule].stick;
          int res = DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_RULE),hdlg,RuleDlgProc,(LPARAM)&subd);
          if (res==IDOK)
          { list<TRule>::iterator ir=add->rules.begin();
            for (int i=0; i<selrule; i++) ir++;
            ir->regexp = subd.re;
            ir->stick = subd.stick;
            SetCursor(LoadCursor(NULL,IDC_WAIT));
            SendMessage(hdlg,WM_APP,0,0);   // update the recents list
            SendMessage(hdlg,WM_APP+1,0,0); // update the rules list
            SendDlgItemMessage(hdlg,IDC_RULES,LB_SETCURSEL,selrule,0);
            SendMessage(hdlg,WM_COMMAND,(WPARAM)(IDC_RULES|(LBN_SELCHANGE<<16)),0);
            SetCursor(LoadCursor(NULL,IDC_ARROW));
          }
        }
      }
      if ((id==IDC_ADD && code==BN_CLICKED) || (id==IDC_RECENTS && code==LBN_DBLCLK))
      { TAssocDlgData subd; subd.pre=add->pre;
        subd.re=make_standard_re("","","");
        subd.stick = reggarble("",true);
        int selrecent = SendDlgItemMessage(hdlg,IDC_RECENTS,LB_GETCURSEL,0,0);
        if (selrecent!=LB_ERR) {subd.re = add->cur_recents[selrecent].regexp; subd.stick = add->cur_recents[selrecent].stick;}
        int selrule = SendDlgItemMessage(hdlg,IDC_RULES,LB_GETCURSEL,0,0);
        if (selrule!=LB_ERR) {subd.re = add->cur_rules[selrule].regexp; subd.stick=add->cur_rules[selrule].stick;}
        int res = DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_RULE),hdlg,RuleDlgProc,(LPARAM)&subd);
        if (res==IDOK)
        { TRule r; r.regexp=subd.re; r.stick=subd.stick; add->rules.push_back(r);
          SetCursor(LoadCursor(NULL,IDC_WAIT));
          SendMessage(hdlg,WM_APP,0,0);   // update the recents list
          SendMessage(hdlg,WM_APP+1,0,0); // update the rules list
          SendDlgItemMessage(hdlg,IDC_RECENTS,LB_SETCURSEL,(WPARAM)-1,0);
          SendDlgItemMessage(hdlg,IDC_RULES,LB_SETCURSEL,add->rules.size()-1,0);
          SendMessage(hdlg,WM_COMMAND,(WPARAM)(IDC_RULES|(LBN_SELCHANGE<<16)),0);
          SetCursor(LoadCursor(NULL,IDC_ARROW));
        }
      }
      if (id==IDOK)
      { SetCursor(LoadCursor(NULL,IDC_WAIT));
        SendMessage(hdlg,WM_APP,TRUE,0); // update add->recents, removing stuff that's matched
        SetCursor(LoadCursor(NULL,IDC_ARROW));
      }
      if (id==IDOK || id==IDCANCEL) EndDialog(hdlg,id);
      return TRUE;
    }
  }
  return FALSE;
}
    


int APIENTRY WinMain(HINSTANCE hi, HINSTANCE, LPSTR, int)
{ hInstance=hi;
  srand(GetTickCount());
  for (int i=0; i<3; i++)
  { HRSRC hrsrc = FindResource(hInstance,MAKEINTRESOURCE(i+1),RT_RCDATA);
    HGLOBAL hglob = LoadResource(hInstance,hrsrc);
    sampledat[i] = (unsigned char*)LockResource(hglob);
    samplesize[i] = SizeofResource(hInstance,hrsrc)/12;
    // the sample data is stored in rows of 12 bytes: 6 for left channel, 6 for right.
  }
  //
  char cfn[MAX_PATH]; *cfn=0; char *c=GetCommandLine();
  if (*c=='"') {c++; while (*c!='"' && *c!=0) c++; if (*c=='"') c++;}
  else {while (*c!=' ' && *c!=0) c++;}
  while (*c==' ') c++;
  bool isedit=(strncmp(c,"/edit",5)==0); if (isedit) c+=6;
  char *d=cfn; if (*c=='"') {c++; while (*c!='"' && *c!=0) {*d=*c; c++; d++;} *d=0;}
  else {while (*c!=' ' && *c!=0) {*d=*c; c++; d++;} *d=0;}
  string fn(cfn);
  string stickdir=GetStickDir();
  //
  bool iszipfile=false, isassocfile=false, isstickfile=false, isbadfile=false, iswrongdirfile=false;
  if (!FileExists(fn)) fn="";
  if (fn!="")
  { iswrongdirfile = !IsFileWithinDirectory(fn,stickdir);
    HANDLE hf=CreateFile(fn.c_str(),GENERIC_READ,FILE_SHARE_READ,NULL,OPEN_EXISTING,0,NULL);
    if (hf==INVALID_HANDLE_VALUE) isbadfile=true; else
    { DWORD magic, red; ReadFile(hf,&magic,4,&red,NULL);
      if (red!=4) isbadfile=true; else
      { if (magic==0x04034b50) iszipfile=true;    // pkzip header
        if (magic==0x6d696c6e) isstickfile=true;  // "nlimbs=..."
        if (magic==0x5a4b5453) iszipfile=true;    // "STKZ - a zipped stick file"
      }
      CloseHandle(hf);
    }
  }

  string fname=ChangeFileExt(StringLower(ExtractFileName(fn)),"");
  if (fname=="associate sticks with music") isassocfile=true;
  fname=StringLower(ExtractFileExt(fn));
  if (fname==".stam") isassocfile=true;

  if (isassocfile)
  { TAssocDlgData add;
    load_rules(&add.rules);
    load_recents(&add.recents);
    vector<TPre> pre; add.pre=&pre;
    int res = DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_ASSOCIATE),NULL,AssocDlgProc,(LPARAM)&add);
    if (res==IDOK) {save_rules(&add.rules); save_recents(&add.recents,true);}
    return res;
  }
    

  if (iszipfile)
  { //FixUpRedundancy(NULL);
    bool wasjustastickfigure=false;
    int res = InstallZipFile(fn.c_str(),&wasjustastickfigure);
    if (!wasjustastickfigure) return res;
    iszipfile=false; isstickfile=true;
  }

  iswrongdirfile &= (GetAsyncKeyState(VK_SHIFT)>=0);
  iswrongdirfile &= !isedit;
  char err[1000];
  string category=""; bool istoonewversion=false;
  if (iswrongdirfile && isstickfile)
  { TBody *b = new TBody(); 
    bool res = LoadBody(&b,fn.c_str(),err,lbForEditing);
    if (!res) isbadfile=true;
    if (b->toonewversion) istoonewversion=true;    
    category=b->category;
    delete b;
  }
  if (iswrongdirfile && isbadfile)
  { string nfn = ExtractFileName(fn);
    string msg = "The file '"+nfn+"' does not seem to be a valid stick figure:\r\n\r\n";
    msg += err;
    MessageBox(NULL,msg.c_str(),"Invalid stick figure",MB_OK);
    return IDCANCEL;
  }
  if (iswrongdirfile && istoonewversion)
  { string msg = "This stick figure requires a more up-to-date version of the plugin.\r\n"
                 "Download it from http://www.wischik.com/lu/senses/sticky";
    MessageBox(NULL,msg.c_str(),"It's time to upgrade!",MB_OK);
    return IDCANCEL;
  }
  if (iswrongdirfile)
  { TInstallItemInfo iii;
    iii.category = category;
    iii.name = CleanWebEscapeChars(ChangeFileExt(ExtractFileName(fn),""));
    if (category=="") iii.desc=iii.name; else iii.desc=category+"\\"+iii.name;
    iii.fn=stickdir+"\\"+iii.desc+".stk";
    iii.srcfn=fn; iii.srczip=0; iii.srczi=0;
    vector<TInstallItemInfo> items; items.push_back(iii);
    TInstallDlgData idd(&items);
    //FixUpRedundancy(NULL);
    return DialogBoxParam(hInstance,MAKEINTRESOURCE(IDD_INSTALL),NULL,InstallDlgProc,(LPARAM)&idd);
  }

  RegLoadUser();
  
  INITCOMMONCONTROLSEX icx; ZeroMemory(&icx,sizeof(icx)); icx.dwSize=sizeof(icx); icx.dwICC=ICC_BAR_CLASSES;
  InitCommonControlsEx(&icx);
  StickClipboardFormat = RegisterClipboardFormat("StickFigure");
  TEditor *editor = new TEditor();

  WNDCLASSEX wcex; ZeroMemory(&wcex,sizeof(wcex)); wcex.cbSize = sizeof(WNDCLASSEX); 
  BOOL res=GetClassInfoEx(hInstance,"Stick3ConfClass",&wcex);
  if (!res)
  { wcex.style = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc = (WNDPROC)EditWndProc;
    wcex.cbClsExtra = 0;
    wcex.cbWndExtra = 0;
    wcex.hInstance = hInstance;
    wcex.hIcon = LoadIcon(NULL,MAKEINTRESOURCE(1));
    wcex.hCursor = LoadCursor(NULL, IDC_ARROW);
    wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW+1);
    wcex.lpszMenuName	= MAKEINTRESOURCE(IDR_MENU);
    wcex.lpszClassName = "Stick3ConfClass";
    wcex.hIconSm = NULL;
    ATOM res=RegisterClassEx(&wcex);
    if (res==0) {MessageBox(NULL,"Failed to register class","Error",MB_OK); return 0;}
  }
  //
  ZeroMemory(&wcex,sizeof(wcex)); wcex.cbSize = sizeof(WNDCLASSEX); 
  res=GetClassInfoEx(hInstance,"Stick3ClientClass",&wcex);
  if (!res)
  { wcex.style = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc = (WNDPROC)ClientWndProc;
    wcex.cbClsExtra = 0;
    wcex.cbWndExtra = 0;
    wcex.hInstance = hInstance;
    wcex.hIcon = 0;
    wcex.hCursor = LoadCursor(NULL, IDC_ARROW);
    wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW+1);
    wcex.lpszMenuName	= 0;
    wcex.lpszClassName = "Stick3ClientClass";
    wcex.hIconSm = NULL;
    ATOM res=RegisterClassEx(&wcex);
    if (res==0) {MessageBox(NULL,"Failed to register class","Error",MB_OK); return 0;}
  }

  //
  editor->mhwnd = CreateWindowEx(0,"Stick3ConfClass", "Sticky Editing",
      WS_OVERLAPPEDWINDOW|WS_CLIPCHILDREN,
      CW_USEDEFAULT, CW_USEDEFAULT, 400, 460, NULL, NULL, hInstance, editor);
  if (editor->mhwnd==NULL) {MessageBox(NULL,"Failed to create window","Error",MB_OK); return 0;}
  hwnd_acc = editor->mhwnd;
  HICON h=(HICON)LoadImage(hInstance,MAKEINTRESOURCE(2),IMAGE_ICON,16,16,0);
  SendMessage(editor->mhwnd,WM_SETICON,ICON_SMALL,(LPARAM)h);

  editor->hstatwnd=CreateWindowEx(0,STATUSCLASSNAME,0,SBARS_SIZEGRIP|WS_VISIBLE|WS_CHILD,0,0,0,0,editor->mhwnd,(HMENU)1,hInstance,0);
  SendMessage(editor->hstatwnd,SB_SIMPLE,TRUE,0);
  editor->chwnd=CreateWindowEx(0,"Stick3ClientClass",0,WS_HSCROLL|WS_VSCROLL|WS_CHILD|WS_VISIBLE,0,0,0,0,editor->mhwnd,(HMENU)2,hInstance,editor);
      
  hac = LoadAccelerators(hInstance,MAKEINTRESOURCE(IDR_ACCELERATORS));
  ShowWindow(editor->mhwnd,SW_SHOW);

  //FixUpRedundancy(editor->mhwnd);
  if (!FileExists(fn)) fn="";
  if (fn=="") {strcpy(editor->curfile,""); editor->ShowHint(""); ShowAngles=true; ShowJoints=true; ShowInvisibles=true;}
  else
  { char err[1000];
    bool res=LoadBody(&editor->body,fn.c_str(),err,lbForEditing);
    if (res)
    { strcpy(editor->curfile,fn.c_str()); editor->tool=tEdit;
      string msg = "Opened file "+ChangeFileExt(ExtractFileName(fn),""); editor->ShowHint(msg);
    }
    else
    { strcpy(editor->curfile,"");
      strcat(err,"\r\nUnable to open file "); strcat(err,fn.c_str());
      MessageBox(editor->mhwnd,err,"Sticky",MB_OK);
      editor->ShowHint("");
    }
  }
  editor->FileReady();

  MSG msg;
  while (GetMessage(&msg, NULL, 0, 0)) 
  { if (!TranslateAccelerator(editor->mhwnd,hac,&msg))
    { TranslateMessage(&msg);
      DispatchMessage(&msg);
    }
  }

  RegSaveUser();
  delete editor;
  return msg.wParam;
}








 // --------------------------------------------------------------------------




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

