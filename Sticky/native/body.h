#ifndef __body_H
#define __body_H


#define STK_CORE       3.74
#define STK_CORE_S    "3.74"
#define STK_VERSION    3,8,0,0
#define STK_VERSION_S "3,8,0,0"


namespace stk {



// Assertion. We don't actually implement luassertmsg. That's left to the application.
void luassertmsg(const char *s,const char *msg,const char *f,unsigned int l);
#define LUASSERT(expr) {if (!(expr)) luassertmsg(#expr,"Failed",__FILE__,__LINE__);}
#define LUASSERTM(expr,msg) {if (!(expr)) luassertmsg(#expr,msg,__FILE__,__LINE__);}
#define LUASSERTMM(msg) {luassertmsg("",msg,__FILE__,__LINE__);}



// String manipulation functions...
string StringTrim(const string s);
string StringInt(int i);
string StringFloat(double d);
string StringUpper(const string s);
string StringLower(const string s);
string ChangeFileExt(const string FileName, const string Extension);
string ExtractFileExt(const string FileName);
string ExtractFileName(const string FileName);
string garble(const string s);
string ungarble(const string s);

struct RGBCOLOR {int r; int g; int b; RGBCOLOR(int r,int g,int b):r(r),g(g),b(b) {} };
struct SIZE {long cx; long cy;};



const double pi=3.141592753589793238462743383279;

enum ColorType {ctNone, ctDefault, ctRGB, ctBitmap, ctEffect};
struct TColor
{ TColor() : type(ctNone), otype(ctRGB), dtype(ctNone), bitmap(""), effect(""), bindex(-1), eindex(-1), rgb(128,128,128), ef(0) {}
  ColorType type,otype;      // if type=ctNone, then otype is the old one (for editing purposes)
  ColorType dtype;           // this gets set every frame by Calc, according to effect, and is ctNone/ctRGB/ctBitmap
  RGBCOLOR rgb;              // for ctRGB
  bool fadetonext;           // for a ctRGB within a 'cols' array.
  string bitmap; int bindex; // for ctBitmap. bindex is a cached index into the bitmap
  string effect; int eindex; double ef; int ereflect; // for ctEffect.
  //
  static string cttostring(ColorType ct);
  static ColorType ctfromstring(const string s);
  string tostring() const;
  void fromstring(const string s);
};
// Note: a line color (limb.color or shape.line) is either ctNone or ctRGB or ctFreq or ctDefault
// body->lines[0].color is ctRGB
// a ColFreq.col is either ctRGB or ctBitmap
// otype is never ctNone.

struct TEffect
{ TEffect() : name("neweffect"), chan(0), band(1), negative(false), cumulative(false), f(0.3), cumrate(1.0), creflect(0) {}
  string name;               // This is a series of segments of a circle. Their size is
  vector<TColor> cols;       // proportional to freq[i] (apart from the last, which is the remainder)
  vector<double> fracs;      // A spring in the center, chan/band, will point to a segment.
  int chan,band; bool negative,cumulative; double cumrate; int creflect;
  int eindex(const double f,double *fstart=0,double *fend=0) const;// given a frac, this figures out which segment it is
  double f;                  // used by the editor. Also taken as the initial position for a cumulative color.
  string tostring() const;
  void fromstring(const string s);
};


  
struct TLimb
{ TLimb() : childi(0), thickness(-1), root(0), chan(0), band(0), cum(false), crate(1), anchor(0), type(0), negative(false), aisoff(true), length(1), linestyle(""), freqstyle("") {}
  //
  int childi;         // index into the 'children' array of all its children
  //
  string linestyle;   // if these are set, then a change to the template will be
  string freqstyle;   // copied into the line
  TColor color;       // color of the line. (ctNone for an invisible limb)
  double thickness;   // a mutiplier of the standard thickness. -1 means "1 pixel"
  // 
  int root;           // where this one is anchored to
  int chan,band;      // which sound source to use. 0=left, 1=right, 2=left-right,
  bool cum;           // 3.0=karaoke-vocals 3.1=karaoke-music 4.0=f 4.1=aoff->aoff+ascale with f=1
  double crate;       // Cum,crate for a cumulative increase in f. Chan must be 0,1,2 or 3.
  int creflect;       // do the cumulatives reflect? 1=upcounting, -1=downcounting, 0=nonreflective. Editor only sets 1 or 0; -
  int anchor;         // 0=none, 1=north, 2=east, 3=south, 4=west
  //
  int type;           // 0=line, 1=arc, 2=spring, 3=circle-spring
  bool negative;      // do we count 1-f or just the normal f
  bool aisoff; double aoff,ascale,aspring; double length; double lmin;
  // type0: a line starting at root, aisoff says whether its angle is relative to the root's,
  //        flexes from angle aoff to aoff+ascale, with length. f says how far towards ascale.
  // type1: an arc with center at root, aisoff the same, radius is length,
  //        arc starts at center+aoff*length and ends at center+(aoff+ascale)*length. f the same.
  // type2: a line starting at root, aisoff the same, going at angle aspring,
  //        length is a fraction f between lmin and length
  // type3: a circle starting at root, radius is a fraction f between lmin and length
  //  
  double x,y, x0,y0; // derived data for current sample: x,y is current endpoint; x0,y0 is startpoint. 
  double ang0,ang,ang1, f; // ang0<=ang<=ang1. 0,1 are extremes (adjusted for parent); ang is current value.
};


struct TJointRef
{ int i; bool ar;
  TJointRef(int ai,bool aar=false):i(ai),ar(aar){}
  TJointRef() : i(-1),ar(false){}
  bool operator==(const TJointRef &b) const {return i==b.i && ar==b.ar;}
  bool operator!=(const TJointRef &b) const {return i!=b.i || ar!=b.ar;}
};
// if ar is true and i is an arc, then the start of the arc.
// note that a shape index from (i,false)->(i,true) will follow the curve of the arc

void jointpos(double *bx,double *by,const TLimb &limb,bool ar,double f=0.0);
// calculates the position of the joint of the limb. For normal
// limbs, this is just limb.x and limb.y. But for arc-roots, it's
// the start of the arc. Also, the optional argument f can find
// positions part way through the arc.


struct TShape
{ TShape() : limbs(false) {}
  bool limbs; // if this is set, then the shape isn't really a shape, but merely shows where the lines come in the Z-order
  vector<TJointRef> p;
  string fillstyle, linestyle; // if either of these are set, then a change to the template style will be copied into the shape
  TColor line; double thickness; // line attributes
  TColor brush; bool balternate; // fill attributes
  // if brushcolor:ctBitmap, but p doesn't have two points on a circle, then behaviour is undefined.
};   

struct TStyle
{ string name;
  TShape shape;
  TLimb limb;
  char shortcut;
  TStyle();
  string tostring() const;
  void fromstring(const string s);
};

struct TBmp
{ TBmp() : buf(0), bufsize(0), bwidth(-1), bheight(-1), hbm(0), hbmask(0), hbcache(0), hbmaskcache(0), cprevi(0) {memset(cprev,0,sizeof(cprev));}
  bool operator<(const TBmp &b) const {return name<b.name;}
  string name; // used at runtime to refer to this Bmp
  char *buf; unsigned int bufsize; string fn; // for saving the file to disk
  void release() {if (buf!=0) delete[] buf; buf=0;}
  int bwidth,bheight; // width and height of the image; the renderer must set these when it loads the images
  
  HBITMAP hbm, hbmask; 
  HBITMAP hbcache,hbmaskcache; int cwidth,cheight; SIZE cprev[5]; int cprevi;
};



enum ReadDataFlags {rdMerge=0, rdOverwrite=1, rdStrict=2};
//
class TBody
{ public:
  TLimb *limbs; int nlimbs;
  vector<int> children;
  vector<TShape> shapes;
  vector<TEffect> effects; int eindex(const string ename);
  vector<TBmp> bmps; int bindex(const string bmpname);
  list<TStyle> styles;
  int fps;
  string category;
  string copyright;
  string version; bool toonewversion; // whether the .stk came from a newer version of the plugin
  TBody();
  ~TBody();

  void AssignFreq(double freqs[3][6], double cmul); // cmul is how many times bigger than 10ms was our last interval
  void Recalc(); double anchx,anchy; // user must add manually, to get (limb.x+anchx, limb.y+anchy).
  void RecalcEffects(bool foredit=false);   // for all effects, assuming their .ef have been set, we assign colors/bitmaps
  void RecalcLimb(int n);
  void RecalcAngles(TLimb *limb);
  int DeleteLimb(int n); int DeleteBranch(int n,vector<int> *remap);
  int CreateLimb(TLimb &limb);
  void MakeChildren();

  // these internal functions are just for the stick-body-data parts
  void NewFile();
  unsigned int WriteData(char *buf,unsigned int bufsize, int root); // returns number of bytes writ
  unsigned int WriteData(char *buf,unsigned int bufsize, list<int> &roots); // returns number of bytes writ
  bool ReadData(const char *buf,char *errbuff,ReadDataFlags flags,list<int> *roots); // returns index of the read root
  static const char *get_coreversion();

  // and these are for the whole body including images
  static TBody* LoadBody(void *buf,unsigned int bufsize, char *err,bool strict);
  void SaveBody(FILE *f);
  void Strip(); // strips away stuff that won't be needed during runtime (but a stripped body can't be saved to disk any more)
};

bool StylesFromString(const char *buf, list<TStyle> &styles);

string ExtractFilePath(const string FileName);
bool IsFilePathRelative(const string fn);
string ExtractFileDir(const string FileName);
string ExtractFileDrive(const string FileName);
string ExpandFileName(const string FileName);
string ExpandUNCFileName(const string FileName);
string ExtractRelativePath(const string BaseName, const string DestName);
string ExtractShortPathName(const string FileName);
bool ForceDirectories(const string dir);
int LastDelimiter(const string Delimiters, const string S);

} // namespace

#endif
