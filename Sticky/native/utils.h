#ifndef __utils_H
#define __utils_H

namespace stk {

string GetLastErrorString();

string GetStickDir(bool justroot=false);

typedef struct {string regexp; string stick;} TRule;
void load_rules(list<TRule> *rules,bool evenifunchanged=true);
void save_rules(list<TRule> *rules);
string match_rule(list<TRule> *rules, const string artist, const string title, const string filename);

string make_music_string(const string artist, const string title, const string filename);

void load_recents(list<string> *recents);
void save_recents(list<string> *recents,bool truncate=false);
void add_recent(list<string> *recents, const string artist, const string title, const string filename, const string stick);

enum LoadBodyFlags {lbForUse=0, lbForEditing=0x01, lbUseDeviceDependent=0x02}; // forediting means strict and we keep the bitmap data
bool LoadBody(stk::TBody **pbody,const char *fn, char *err,LoadBodyFlags flags);
bool LoadBodyZip(TBody **pbody,void *hz,int index, char *err,LoadBodyFlags flags);

unsigned int StylesToString(char *buf,unsigned int blen,list<TStyle> &styles);


struct TPre
{ string path,desc,regexp;
  const bool operator<(const TPre &b) {return (desc<b.desc || (desc==b.desc && path<b.path));}
};
//
void DirScan(vector<TPre> &pre, string root, bool showwildcards=false);
int ChooseRandomBody(const string regexp, const vector<TPre> &pre, const string avoid);

HBITMAP MakeJpeg(HGLOBAL hglob,bool usedibsections);
void PrepareBitmapData(TBmp *bmp,bool usedibsections);

void MakeBindexes(TBody *body);
void MakeEindexes(TBody *body);


typedef struct {double *l,*r,*max,*min,*k,*kmin,*kmax;} TAmpData;
bool SimpleDraw(HDC hdc,RECT &rc,TBody *body,const char *ban=0,TAmpData *ad=0, string *err=0);

void FitText(HDC hdc,int defheight, RECT *rc,const string t);

class TEffPt
{ POINT stockpt[1000]; int stockpi[1000];
  public:
  POINT *pt; int *pi; int ptlen;
  TEffPt() : pt(stockpt),pi(stockpi),ptlen(1000) {}
  ~TEffPt() {if (pt!=stockpt) delete[] pt; if (pi!=stockpi) delete[] pi;}
  void ensure(int asize)
  { if (asize<ptlen) return;
    int newlen=2*asize; POINT *ptnew=new POINT[newlen]; int *pinew=new int[newlen];
    memcpy(ptnew,pt,sizeof(POINT)*ptlen); memcpy(pinew,pi,sizeof(int)*ptlen);
    if (pt!=stockpt) delete[] pt; if (pi!=stockpi) delete[] pi;
    pt=ptnew; pi=pinew; ptlen=newlen;
  }
  int add(int optpos,int x,int y,int id)
  { ensure(optpos+1); pt[optpos].x=x; pt[optpos].y=y; pi[optpos]=id; return optpos+1;
  }
};

typedef int (*b2func)(void *dat,double b);

int add_pt(b2func pb2x,b2func pb2y,void *dat,TEffPt *ept,int ptpos,double x,double y,int id);
int add_pt(b2func pb2x,b2func pb2y,void *dat,TEffPt *ept,int ptpos,TLimb &limb,bool ar,int id);
int add_arc(b2func pb2x,b2func pb2y,void *dat,TEffPt *ept,int ptpos,TLimb &limb,bool forwards,int id);


void HideRedundancy(vector<TPre> &pre);


void PopulateMenu(HMENU hmenu,const char *desc,int id,bool checked);



} // namespace

#endif
