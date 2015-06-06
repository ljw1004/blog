#define STRICT
#define _NO_VCL
#include <windows.h>
#include <wtypes.h>
#pragma warning( push )
#pragma warning( disable: 4786 4702 )
#include <string>
#include <vector>
#include <list>
#include <map>
#include <algorithm>
using namespace std;
#pragma warning( pop )
#include <stdio.h>
#include "../unzip.h"
#include "../body.h"
#include "../utils.h"
using namespace stk;


// This program runs through all the .stk, .sticks files in the
// current directory. It puts them into a file "extradb.txt"
// which contains: filename / category / by
// If the db already contained that filename, its category
// might be overwritten, but its "by" will not.

struct dbitem {string fn; string cat; string back; string by;};
vector<dbitem> db;
map<string,int> dblookup;

bool printed=false;


void SaveBitmap(TBody *b,const string fn)
{ double freq[2][6];
  freq[2][6];
  freq[0][0]=0.6;   freq[1][0]=0.58;
  freq[0][1]=0.4;   freq[1][1]=0.38;
  freq[0][2]=0.8;   freq[1][2]=0.2;
  freq[0][3]=0.4;   freq[1][3]=0.3;
  freq[0][4]=0.7;   freq[1][4]=0.7;
  freq[0][5]=0.3;   freq[1][5]=0.8;
  for (int n=0; n<b->nlimbs; n++)
  { int chan=b->limbs[n].chan, band=b->limbs[n].band;
    double f; if (chan==0 || chan==1) f=freq[chan][band];
    else if (chan==2) f=(1.0+freq[0][band]-freq[1][band])*0.5;
    else f=b->limbs[n].f;
    if (chan!=4) {if (b->limbs[n].negative) f=1.0-f;}
    b->limbs[n].f = f;
  }
  b->Recalc();

  HDC sdc=GetDC(0); HDC hdc=CreateCompatibleDC(sdc); 
  //
  BITMAPINFOHEADER bih; ZeroMemory(&bih,sizeof(bih));
  bih.biSize=sizeof(bih);
  bih.biWidth=800;
  bih.biHeight=800;
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
  //
  RECT rc; rc.top=0; rc.left=0; rc.bottom=800; rc.right=800;
  COLORREF clrBackground=b->limbs[0].color.rgb;
  HBRUSH hbr = CreateSolidBrush(clrBackground); 
  FillRect(hdc,&rc,hbr);
  DeleteObject(hbr);
  rc.top=50; rc.left=50; rc.bottom=750; rc.right=750;
  SimpleDraw(hdc,rc,b,0,0);

  HANDLE hf = CreateFile(fn.c_str(),GENERIC_WRITE,0,NULL,CREATE_ALWAYS,FILE_ATTRIBUTE_NORMAL,NULL);
  BITMAPFILEHEADER bfh; ZeroMemory(&bfh,sizeof(bfh));
  bfh.bfType=0x4d42;
  bfh.bfSize=sizeof(bfh)+sizeof(bih)+bih.biSizeImage;
  bfh.bfReserved1=0;
  bfh.bfReserved2=0;
  bfh.bfOffBits=sizeof(bfh)+sizeof(bih);
  DWORD writ;
  WriteFile(hf,&bfh,sizeof(bfh),&writ,NULL);
  WriteFile(hf,&bih,sizeof(bih),&writ,NULL);
  WriteFile(hf,bits,bih.biSizeImage,&writ,NULL);
  CloseHandle(hf);

  SelectObject(hdc,hold);
  DeleteObject(hbm);
  DeleteDC(hdc);
  ReleaseDC(0,sdc);
}


void Insert(const string fn,const string cat,TBody *b)
{ map<string,int>::const_iterator i = dblookup.find(StringLower(fn));
  if (i==dblookup.end())
  { dbitem it; it.fn=fn; it.cat=cat;
    const char *c=fn.c_str();
    while (*c!='(' && *c!=0) c++; if (*c=='(') c++;
    const char *d=c; while (*d!=')' && *d!=0) d++;
    string by(c,d-c); by="by "+by+"."; it.by=by;
    it.back="";
    int pos = (int)db.size();
    db.push_back(it);
    dblookup.insert(pair<const string,int>(StringLower(fn),pos));
    printf("\nadded: %s - %s - %s",it.fn.c_str(),it.cat.c_str(),it.by.c_str());
    printed=true;
  }
  else
  { string oldcat = db[i->second].cat;
    if (oldcat!=cat)
    { db[i->second].cat=cat;
      printf("\nrecat: %s - %s -> %s",fn.c_str(),oldcat.c_str(),cat.c_str());
      printed=true;
    }
  }

  // we might have to make a picture from it
  string gif = ChangeFileExt(fn,".gif");
  bool fres = (GetFileAttributes(gif.c_str())!=0xFFFFFFFF);
  if (!fres && b!=NULL) SaveBitmap(b,ChangeFileExt(fn,".bmp"));
}

void stk::luassertmsg(const char *s,const char *msg,const char *f,unsigned int l)
{ printf("\n%s '%s' - %s:%u",msg,s,f,l);
  printed=true;
}




int main(int argc, char* argv[])
{ string fndb = "extradb.txt";
  if (argc==2) fndb=argv[1];
 
 
  // First load in the database, if it exists
  HANDLE hf = CreateFile(fndb.c_str(),GENERIC_READ,FILE_SHARE_READ,NULL,OPEN_EXISTING,0,NULL);
  if (hf!=INVALID_HANDLE_VALUE)
  { DWORD size=GetFileSize(hf,NULL);
    char *buf = new char[size+1];
    DWORD red; ReadFile(hf,buf,size,&red,NULL); buf[size]=0;
    CloseHandle(hf);
    const char *c=buf;
    while (*c!=0)
    { while (*c==' ') c++; if (*c=='\r') c++;
      if (*c=='\n') {c++; continue;}
      const char *start=c;
      while (*c!='\t' && *c!='\r' && *c!='\n' && *c!=0) c++;
      const char *tab1=c; if (*c=='\t') c++;
      while (*c!='\t' && *c!='\r' && *c!='\n' && *c!=0) c++;
      const char *tab2=c; if (*c=='\t') c++;
      while (*c!='\t' && '\r' && *c!='\n' && *c!=0) c++;
      const char *tab3=c; if (*c=='\t') c++;
      while (*c!='\r' && *c!='\n' && *c!=0) c++;
      const char *end=c;
      dbitem it;
      if (start!=0 && tab1>start) it.fn=string(start,tab1-start);
      if (tab1!=0 && tab2>tab1) it.cat=string(tab1+1,tab2-tab1-1);
      if (tab2!=0 && tab3>tab2) it.back=string(tab2+1,tab3-tab2-1);
      if (tab3!=0 && end>tab3) it.by=string(tab3+1,end-tab3-1);
      int pos = (int)db.size();
      db.push_back(it);
      dblookup.insert(pair<const string,int>(StringLower(it.fn),pos));
    }
    delete[] buf;
  }

  bool ondots=true;

  //
  // Now go through the files
  WIN32_FIND_DATA fdat; HANDLE hfind = FindFirstFile("*.stk",&fdat);
  while (hfind!=INVALID_HANDLE_VALUE)
  { printed=false;
    TBody *b = new TBody(); string fn=fdat.cFileName; char err[1000];
    bool bres = LoadBody(&b,fn.c_str(),err,lbForUse);
    if (bres)
    { string cat = b->category;
      Insert(fn,cat,b);
    }
    delete b;
    if (printed) ondots=false;
    if (!printed && !ondots) printf("\n");
    if (!printed) {printf("."); ondots=true;}
    BOOL res = FindNextFile(hfind,&fdat);
    if (!res) {FindClose(hfind); hfind=INVALID_HANDLE_VALUE;}
  } 

  // Now go through the zip-files
  hfind = FindFirstFile("*.sticks",&fdat);
  while (hfind!=INVALID_HANDLE_VALUE)
  { printed=false;
    string fn(fdat.cFileName); string cat("");
    HZIP hz = OpenZip((void*)fn.c_str(),0,ZIP_FILENAME);
    ZIPENTRY ze; GetZipItem(hz,-1,&ze); int numentries=ze.index; int largest=-1, large_size=-1;
    for (int i=0; i<numentries; i++)
    { GetZipItem(hz,i,&ze);
      if (StringLower(ExtractFileExt(ze.name))==".stk" && ze.unc_size>large_size) {largest=i; large_size=ze.unc_size;}
    }
    if (largest==-1) {printf("\nerror in zip '%s'",fn.c_str()); printed=true;}
    else
    { TBody *b = new TBody(); char err[1000];
      bool bres = LoadBodyZip(&b,hz,largest,err,lbForUse);
      if (bres)
      { string cat=b->category;
        if (numentries>2) // we assume that it's flat if it's smaller
        { const char *start=b->category.c_str(), *c=start, *lastslash=c+strlen(c);
          while (*c!=0) {if (*c=='\\' || *c=='/') lastslash=c; c++;}
          cat=string(start,lastslash-start);
        }
        Insert(fn,cat,b);
      }
      delete b;
    }
    CloseZip(hz);
    //
    if (printed) ondots=false;
    if (!printed && !ondots) printf("\n");
    if (!printed) {printf("."); ondots=true;}

    BOOL res = FindNextFile(hfind,&fdat);
    if (!res) {FindClose(hfind); hfind=INVALID_HANDLE_VALUE;}
  } 

  printf("\n");

  // now write the database
  FILE *f = fopen(fndb.c_str(),"wt");
  for (vector<dbitem>::const_iterator i=db.begin(); i!=db.end(); i++)
  { fprintf(f,"%s\t%s\t%s\t%s\n",i->fn.c_str(),i->cat.c_str(),i->back.c_str(),i->by.c_str());
  }
  fclose(f);



  return 0;
}