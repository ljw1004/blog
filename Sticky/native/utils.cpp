#include <windows.h>
#include <ole2.h>
#include <ocidl.h>
#include <olectl.h>
#pragma warning( push )
#pragma warning( disable: 4786 4702 )
#include <string>
#include <vector>
#include <list>
#include <algorithm>
#include <map>
using namespace std;
#pragma warning( pop )
#include <math.h>
#include "body.h"
#include "utils.h"
#include "unzip.h"
#include "regex.h"


const double LineThickness = 0.03;


namespace stk {


string GetLastErrorString()
{ LPVOID lpMsgBuf;
  FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,NULL,
    GetLastError(),MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPTSTR) &lpMsgBuf, 0, NULL);
  string s((char*)lpMsgBuf);
  LocalFree( lpMsgBuf );
  return s;
}

string GetStickDir(bool justroot)
{ char c[MAX_PATH]; c[0]=0; DWORD type,size; HKEY hkey;
  LONG res = RegOpenKeyEx(HKEY_LOCAL_MACHINE,"Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\StickEd.exe",0,KEY_READ,&hkey);
  if (res==ERROR_SUCCESS)
  { size=MAX_PATH; res=RegQueryValueEx(hkey,"Path",0,&type,(LPBYTE)c,&size);
    RegCloseKey(hkey);
  }
  string r;
  if (c[0]!=0) r=c;
  else {GetModuleFileName(NULL,c,MAX_PATH); r=ExtractFilePath(c);}
  if (!justroot) r+="\\sticks";
  return r;
}



void load_rules(list<TRule> *rules,bool evenifunchanged)
{ HKEY key=NULL; char dat[10000]; DWORD ddat, size, type;
  LONG res=RegOpenKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,KEY_READ,&key);
  if (res!=ERROR_SUCCESS) return;
  if (!evenifunchanged)
  { size=sizeof(ddat); res=RegQueryValueEx(key,"RulesChanged",NULL,&type,(LPBYTE)&ddat,&size);
    bool ischanged = (res==ERROR_SUCCESS && ddat!=0);
    if (!ischanged) {RegCloseKey(key); return;}
    ddat=0; RegSetValueEx(key,"RulesChanged",0,REG_DWORD,(LPBYTE)&ddat,sizeof(ddat));
  }
  rules->clear();
  for (int i=0; ; i++)
  { char vn[20],vm[20]; wsprintf(vn,"rule%i-src",i); wsprintf(vm,"rule%i-dst",i);
    TRule r;
    size=sizeof(dat); *dat=0; res=RegQueryValueEx(key,vn,NULL,&type,(LPBYTE)&dat,&size);
    if (res!=ERROR_SUCCESS || *dat==0) break;
    r.regexp=dat;
    size=sizeof(dat); *dat=0; res=RegQueryValueEx(key,vm,NULL,&type,(LPBYTE)&dat,&size);
    if (res!=ERROR_SUCCESS || *dat==0) break;
    r.stick=dat;
    rules->push_back(r);
  }
  RegCloseKey(key);
}

void save_rules(list<TRule> *rules)
{ // first we'll retrieve the existing rule list. So we know how big it is.
  // So we know if we have to delete any rules ourselves.
  list<TRule> oldrules; load_rules(&oldrules); int oldsize=(int)oldrules.size();
  //
  HKEY key; DWORD disp; LONG res;
  res=RegCreateKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,NULL,0,KEY_WRITE,NULL,&key,&disp);
  if (res!=ERROR_SUCCESS) return;
  int i=0;
  for (list<TRule>::const_iterator ir=rules->begin(); ir!=rules->end(); ir++,i++)
  { char vn[20],vm[20]; wsprintf(vn,"rule%i-src",i); wsprintf(vm,"rule%i-dst",i);
    RegSetValueEx(key,vn,0,REG_SZ,(CONST BYTE*)ir->regexp.c_str(),(int)ir->regexp.length()+1);
    RegSetValueEx(key,vm,0,REG_SZ,(CONST BYTE*)ir->stick.c_str(),(int)ir->stick.length()+1);
  }
  for (; i<oldsize; i++)
  { char vn[20],vm[20]; wsprintf(vn,"rule%i-src",i); wsprintf(vm,"rule%i-dst",i);
    RegDeleteValue(key,vn);
    RegDeleteValue(key,vm);
  }
  DWORD ddat=1; RegSetValueEx(key,"RulesChanged",0,REG_DWORD,(LPBYTE)&ddat,sizeof(ddat));
  RegCloseKey(key);
}

string make_music_string(const string artist, const string title, const string filename)
{ return "ARTIST="+artist+"___TITLE="+title+"___FILENAME="+filename+"___";
}


string match_rule(list<TRule> *rules, const string artist, const string title, const string filename)
{ string s = make_music_string(artist,title,filename);
  for (list<TRule>::const_iterator it=rules->begin(); it!=rules->end(); it++)
  { REGEXP re; regbegin(&re,it->regexp.c_str(),REG_ICASE);
    bool res = regmatch(&re,s.c_str(),NULL,0);
    regend(&re);
    if (res) return it->stick;
  }
  return "";
} 

const int max_recents = 20;

void load_recents(list<string> *recents)
{ recents->clear();
  HKEY key=NULL; char dat[10000]; DWORD size; DWORD type;
  LONG res=RegOpenKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,KEY_READ,&key);
  if (res!=ERROR_SUCCESS) return;
  for (int i=0; i<max_recents; i++)
  { char vn[20]; wsprintf(vn,"recent%i",i);
    size=sizeof(dat); *dat=0; res=RegQueryValueEx(key,vn,NULL,&type,(LPBYTE)&dat,&size);
    if (res!=ERROR_SUCCESS) break;
    recents->push_back(dat);
  }
  RegCloseKey(key);
}
  
void save_recents(list<string> *recents,bool truncate)
{ HKEY key; DWORD disp; 
  LONG res=RegCreateKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,NULL,0,KEY_WRITE,NULL,&key,&disp);
  if (res!=ERROR_SUCCESS) return;
  int i=0;
  for (list<string>::const_iterator it=recents->begin(); it!=recents->end(); it++,i++)
  { char vn[20]; wsprintf(vn,"recent%i",i);
    const string &s = *it;
    RegSetValueEx(key,vn,0,REG_SZ,(CONST BYTE*)s.c_str(),(int)s.length()+1);
  }
  for (; truncate && i<max_recents; i++)
  { char vn[20]; wsprintf(vn,"recent%i",i);
    RegDeleteValue(key,vn);
  }
  RegCloseKey(key);
}

void add_recent(list<string> *recents, const string artist, const string title, const string filename, const string stick)
{ string s = make_music_string(artist,title,filename);  size_t len=s.length();
  list<string>::iterator found=recents->end();
  for (list<string>::iterator it=recents->begin(); it!=recents->end() && found==recents->end(); it++)
  { if (_strnicmp(it->c_str(),s.c_str(),len)==0) found=it;
  }
  s += "STICK="+stick+"___";
  if (found!=recents->end())
  { recents->erase(found);
    recents->push_front(s);
  }
  else
  { while (recents->size()>=max_recents) recents->pop_back();
    recents->push_front(s);
  }
}




int AnyChristmasSticks=-1; // -1=dunno 0=no >0=yes
int IsItNearChristmas=-1;  // -1=dunno 0=no 1=december 2=christmasday

int ChooseRandomBody(const string sregexp, const vector<TPre> &pre, const string avoid)
{ const char *regexp=sregexp.c_str();
  if (IsItNearChristmas==-1)
  { SYSTEMTIME st; GetSystemTime(&st);
    if (st.wMonth==12) IsItNearChristmas=1;
    else if (st.wMonth==12 && st.wDay>=23 && st.wDay<=26) IsItNearChristmas=2;
    else IsItNearChristmas=0;
  }
  bool tryingforchristmas=false;
  if (AnyChristmasSticks!=0 && IsItNearChristmas>0 && (sregexp=="" || sregexp==".*"))
  { if (IsItNearChristmas==1 && (rand()%1000)<250) {regexp=".*hristmas.*"; tryingforchristmas=true;}
    if (IsItNearChristmas==2) {regexp=".*hristmas.*"; tryingforchristmas=true;}
  }
  vector<int> possibilities;
  if (tryingforchristmas || (sregexp!="" && sregexp!=".*"))
  { possibilities.reserve(pre.size());
    REGEXP re; regbegin(&re,regexp,0);
    for (int i=0; i<(int)pre.size(); i++)
    { bool match = regmatch(&re,pre[i].desc.c_str(),NULL,0);
      if (match) possibilities.push_back(i);    
    }
  }
  if (tryingforchristmas) AnyChristmasSticks = (int)possibilities.size();
  //
  // that's so that, if our attempted regexp search for christmas bore no
  // fruit, then we won't try again. It seems a bit harsh to penalise people
  // for having no christmas sticks!
  //
  if (possibilities.size()>0)
  { return possibilities[rand()%possibilities.size()];
  }
  else
  { return rand()%(int)pre.size();
  }
}




struct CatItem
{ string name; // for a directory, this is its pure name. For a stick file, pure name without suffix
  bool isdir;  
  string fn;   // for a stick file, this is its full filename
  list<int> items; // for a directory, these are the items in it
  int allitems;    // for a directory, the count of all sticks here and in subbranches
  bool collapsed;  // this says whether we have been collapsed into a higher node
  bool operator< (const CatItem &b) const
  { size_t alen=name.length(), blen=b.name.length(), minlen=min(alen,blen);
    int cmp = _strnicmp(name.c_str(),b.name.c_str(),minlen);
    if (cmp<0) return true;
    if (cmp>0) return false;
    return alen<blen;
  }
  bool operator== (const CatItem &b) const {return name==b.name;}
};


int RecGetCats(vector<CatItem> &items, const string fnroot, int rpos)
{ string mask = fnroot+"*";
  int tot=0;
  WIN32_FIND_DATA fdat; 
  HANDLE hfind=FindFirstFile(mask.c_str(),&fdat);
  for (BOOL res=(hfind!=INVALID_HANDLE_VALUE); res; res=FindNextFile(hfind,&fdat))
  { bool isdir = (fdat.dwFileAttributes&FILE_ATTRIBUTE_DIRECTORY)!=0;
    bool isstk = (StringLower(ExtractFileExt(fdat.cFileName))==".stk");
    if (strcmp(fdat.cFileName,".")==0) isdir=false;
    if (strcmp(fdat.cFileName,"..")==0) isdir=false;
    if (!isstk && !isdir) continue;
    int cpos=(int)items.size(); CatItem ci; ci.name=ChangeFileExt(ExtractFileName(fdat.cFileName),""); ci.collapsed=false;
    ci.isdir=isdir; if (isstk && !isdir) ci.fn=fnroot+fdat.cFileName;
    items.push_back(ci);
    items[rpos].items.push_back(cpos);
    if (isdir) tot+=RecGetCats(items,fnroot+fdat.cFileName+"\\",cpos);
    else tot++;
  }
  if (hfind!=INVALID_HANDLE_VALUE) FindClose(hfind);
  items[rpos].allitems = tot;
  items[rpos].items.sort();
  items[rpos].items.unique();
  return tot;
}

void collapse_smalls(vector<CatItem> &items, int rpos)
{ CatItem &c = items[rpos];
  if (!c.isdir) return;
  // first, collapse all children
  for (list<int>::const_iterator i=c.items.begin(); i!=c.items.end(); i++) collapse_smalls(items,*i);
  // then, if any of them are too small, merge them in with us
  for (list<int>::iterator i=c.items.begin(); i!=c.items.end();)
  { CatItem &sc = items[*i];
    if (!sc.isdir) {i++; continue;}
    if (sc.allitems>=6) {i++; continue;}
    // invariant: if it had 5 or fewer things, then it has no subdirectories
    i=c.items.erase(i); // disconnect the subdirectory
    for (list<int>::iterator j=sc.items.begin(); j!=sc.items.end(); j++)
    { items[*j].collapsed=true;
      i=c.items.insert(i,*j); i++;
    } 
    // that has copied the sublist into our own list, and incremented our iterator past the end
    // note: copying sublists does not alter 'allitems'
  }
}


void make_pre_list(vector<TPre> &pre, const vector<CatItem> &items, const string prefix, int rpos, bool showwildcards)
{ const CatItem &c = items[rpos];
  string n=prefix+c.name, nd=n+"\\"; if (n=="") nd="";
  if (!c.isdir)
  { TPre p; p.desc=n; p.path=c.fn; p.regexp=p.desc;
    if (showwildcards && c.collapsed) p.regexp=ExtractFilePath(n)+"*\\"+c.name;
    pre.push_back(p);
    return;
  }
  if (showwildcards) 
  { TPre p; p.desc=nd+"*"; p.path=""; pre.push_back(p);
  }
  for (list<int>::const_iterator i=c.items.begin(); i!=c.items.end(); i++)
  { make_pre_list(pre,items,nd,*i,showwildcards);
  }
}

// DirScan: a recursive search of files. Root is "c:\progra~1\stick figures\sticks\" with
// the trailing slash. Showwildcards is for use by the editor: it means that the first
// item in every directory will be a * option
void DirScan(vector<TPre> &pre, string root, bool showwildcards)
{ vector<CatItem> items;
  CatItem ci; ci.name=""; ci.isdir=true; items.push_back(ci);
  RecGetCats(items,root,0);
  if (!showwildcards) collapse_smalls(items,0);
  make_pre_list(pre,items,"",0,showwildcards);
}



HBITMAP MakeJpeg(HGLOBAL hglob,bool usedibsections)
{ IStream *stream=0; HRESULT hr=CreateStreamOnHGlobal(hglob,FALSE,&stream);
  if (!SUCCEEDED(hr) || stream==0) return 0;
  IPicture *pic;  hr=OleLoadPicture(stream, 0, FALSE, IID_IPicture, (LPVOID*)&pic);
  stream->Release();
  if (!SUCCEEDED(hr) || pic==0) return 0;
	HBITMAP hbm0=0; hr=pic->get_Handle((OLE_HANDLE*)&hbm0);
  if (!SUCCEEDED(hr) || hbm0==0) {pic->Release(); return 0;}
  //
  // Now we make a copy of it into our own hbm
  DIBSECTION dibs; GetObject(hbm0,sizeof(dibs),&dibs);
  if (dibs.dsBm.bmBitsPixel!=24) {pic->Release(); return 0;}
  int w=dibs.dsBm.bmWidth, h=dibs.dsBm.bmHeight;
  dibs.dsBmih.biClrUsed=0; dibs.dsBmih.biClrImportant=0; void *bits;
  HDC sdc=GetDC(0);
  HBITMAP hbm1;
  if (usedibsections) hbm1=CreateDIBSection(sdc,(BITMAPINFO*)&dibs.dsBmih,DIB_RGB_COLORS,&bits,0,0);
  else hbm1=CreateDIBitmap(sdc,&dibs.dsBmih,0,0,0,DIB_RGB_COLORS);
  //
  HDC hdc0=CreateCompatibleDC(sdc), hdc1=CreateCompatibleDC(sdc);
  HGDIOBJ hold0=SelectObject(hdc0,hbm0), hold1=SelectObject(hdc1,hbm1);
  BitBlt(hdc1,0,0,w,h,hdc0,0,0,SRCCOPY);
  SelectObject(hdc0,hold0); SelectObject(hdc1,hold1);
  DeleteDC(hdc0); DeleteDC(hdc1);
  ReleaseDC(0,sdc);
  pic->Release();
  return hbm1;
}


void PrepareBitmapData(TBmp *bmp,bool usedibsections)
{ if (bmp->hbm!=0) DeleteObject(bmp->hbm); bmp->hbm=0; bmp->bwidth=0; bmp->bheight=0;
  if (bmp->hbmask!=0) DeleteObject(bmp->hbmask); bmp->hbmask=0;
  bool hastransparency=true;
  if (bmp->name.size()>=3 && StringLower(bmp->name.substr(bmp->name.size()-3))=="-nt")
  { hastransparency=false;
  }
  //
  // Different loading for bmp vs jpeg
  bool isbmp = (bmp->buf[0]=='B' && bmp->buf[1]=='M');
  if (isbmp)
  { BITMAPFILEHEADER *bfh=(BITMAPFILEHEADER*)bmp->buf;
    BITMAPINFOHEADER *bih=(BITMAPINFOHEADER*)(bmp->buf+sizeof(BITMAPFILEHEADER));
    int ncols=bih->biClrUsed; if (ncols==0) ncols=1 << bih->biBitCount;
    char *sbits = (char*)(bmp->buf+bfh->bfOffBits);
    if (usedibsections)
    { char *dbits; bmp->hbm=CreateDIBSection(NULL,(BITMAPINFO*)bih,DIB_RGB_COLORS,(void**)&dbits,NULL,0);
      unsigned int numbytes = bih->biSizeImage;
      if (numbytes==0) numbytes = ((bih->biWidth*bih->biBitCount/8+3)&0xFFFFFFFC)*bih->biHeight;
      CopyMemory(dbits,sbits,numbytes);
    }
    else
    { HDC hdc = GetDC(0);
      bmp->hbm = CreateDIBitmap(hdc,bih,CBM_INIT,sbits,(BITMAPINFO*)bih,DIB_RGB_COLORS);
      ReleaseDC(0,hdc);
    }
  }
  else
  { DWORD size = bmp->bufsize;
    HGLOBAL hglob = GlobalAlloc(GMEM_MOVEABLE,size);
	  void *buf = GlobalLock(hglob); memcpy(buf,bmp->buf,size);
	  GlobalUnlock(hglob);
    bmp->hbm = MakeJpeg(hglob,usedibsections);
    GlobalFree(hglob);
  }
  //
  // Basic information:
  BITMAP b; GetObject(bmp->hbm,sizeof(BITMAP),&b);
  bmp->bwidth=b.bmWidth; bmp->bheight=b.bmHeight;
  if (!hastransparency) return;
  //
  // And now create a mask for it
  HDC screendc=GetDC(0);
  HDC bitdc=CreateCompatibleDC(screendc);
  HGDIOBJ holdb = SelectObject(bitdc,bmp->hbm);
  SetBkColor(bitdc,RGB(0,0,0));
  //
  if (usedibsections)
  { typedef struct {BITMAPINFOHEADER bmiHeader; RGBQUAD bmiColors[2];} MONOBITMAPINFO;
    MONOBITMAPINFO bmi; ZeroMemory(&bmi,sizeof(bmi));
    bmi.bmiHeader.biSize=sizeof(BITMAPINFOHEADER);
    bmi.bmiHeader.biWidth=bmp->bwidth;
    bmi.bmiHeader.biHeight=bmp->bheight;
    bmi.bmiHeader.biPlanes=1;
    bmi.bmiHeader.biBitCount=1;
    bmi.bmiHeader.biCompression=BI_RGB;
    bmi.bmiHeader.biSizeImage=((bmp->bwidth+7)&0xFFFFFFF8)*bmp->bwidth/8;
    bmi.bmiHeader.biXPelsPerMeter=1000000;
    bmi.bmiHeader.biYPelsPerMeter=1000000;
    bmi.bmiHeader.biClrUsed=0;
    bmi.bmiHeader.biClrImportant=0;
    bmi.bmiColors[0].rgbRed=0;  bmi.bmiColors[0].rgbGreen=0;  bmi.bmiColors[0].rgbBlue=0;  bmi.bmiColors[0].rgbReserved=0;
    bmi.bmiColors[1].rgbRed=255;bmi.bmiColors[1].rgbGreen=255;bmi.bmiColors[1].rgbBlue=255;bmi.bmiColors[1].rgbReserved=0;
    void *dbits;  bmp->hbmask=CreateDIBSection(screendc,(BITMAPINFO*)&bmi,DIB_RGB_COLORS,&dbits,NULL,0);
    // I don't know how to create masks! The following code seems to work.
    // It blts the image onto a monocrome DDB, and then blts this onto the DIB.
    // (if I tried blting from the image straight onto the mask DIB, then it
    // chose the closest mask colour, not the absolute mask colour).
    HDC monodc=CreateCompatibleDC(screendc);
    HDC maskdc=CreateCompatibleDC(screendc);
    HBITMAP hmonobm = CreateBitmap(bmp->bwidth,bmp->bheight,1,1,NULL);
    HGDIOBJ holdm = SelectObject(monodc,hmonobm);
    HGDIOBJ holdmask = SelectObject(maskdc,bmp->hbmask);
    COLORREF transp = GetPixel(bitdc,0,0);
    SetBkColor(bitdc,transp); // use top-left pixel as transparent colour
    BitBlt(monodc,0,0,bmp->bwidth,bmp->bheight,bitdc,0,0,SRCCOPY);
    BitBlt(maskdc,0,0,bmp->bwidth,bmp->bheight,monodc,0,0,SRCCOPY);
    // the mask has 255 for the masked areas, and 0 for the real image areas.
    // Well, that has created the mask. Now we have to zero-out the original bitmap's masked area
    BitBlt(bitdc,0,0,bmp->bwidth,bmp->bheight,monodc,0,0,0x00220326);
    // 0x00220326 is the ternary raster operation 'DSna', which is reverse-polish for
    // bitdc AND (NOT monodc)
    SelectObject(maskdc,holdmask); DeleteDC(maskdc);
    SelectObject(monodc,holdm); DeleteDC(monodc); DeleteObject(hmonobm);
  }
  else
  { bmp->hbmask = CreateBitmap(bmp->bwidth,bmp->bheight,1,1,NULL);
    HDC maskdc=CreateCompatibleDC(screendc);
    HGDIOBJ holdm = SelectObject(maskdc,bmp->hbmask);
    COLORREF transp = GetPixel(bitdc,0,0);
    SetBkColor(bitdc,transp); // use top-left pixel as transparent colour
    BitBlt(maskdc,0,0,bmp->bwidth,bmp->bheight,bitdc,0,0,SRCCOPY);
    BitBlt(bitdc,0,0,bmp->bwidth,bmp->bheight,maskdc,0,0,0x00220326);
    SelectObject(maskdc,holdm); DeleteDC(maskdc);
  }
  // 
  SelectObject(bitdc,holdb); DeleteDC(bitdc);
  ReleaseDC(0,screendc);
}





bool LoadBodyMem(TBody **pbody,char *buf,unsigned int bufsize, char *err,LoadBodyFlags flags)
{ bool forediting = (flags&lbForEditing)!=0;
  bool usedibsections = (flags&lbUseDeviceDependent)==0;
  TBody *body = *pbody;
  // First, a quick check: check the first thousand bytes for "\nversion=" and
  // see if the version is high enough. If it is, we will load it as a zip. If not, not.
  if (bufsize<=10) return false;
  DWORD magic = * ((DWORD*)buf);
  bool iszipfile = (magic==0x04034b50);
  bool isstickfile = (magic==0x6d696c6e);
  if (!iszipfile && !isstickfile) return false;
  if (isstickfile)
  { unsigned int headscan=1000; if (bufsize-10<headscan) headscan=bufsize-10;
    const char *c=0;
    for (unsigned int i=0; i<headscan; i++)
    { if (strncmp(buf+i,"\nversion=",9)==0) {c=buf+i; break;}
    }
    double sver=3.4; if (c!=0) {c+=9; while (*c==' ') c++; if (*c!=0) sscanf(c,"%lg",&sver);}
    isstickfile = (sver<=3.6);
  }
  //
  if (isstickfile) return body->ReadData(buf,err,rdOverwrite,NULL);
  //
  // otherwise, it was a new version, zipped up, maybe with a zip-sfx header
  // now we can do the zip
  TBody *newbody = new TBody();
  HZIP hz = OpenZip(buf,bufsize,NULL);
  ZIPENTRY ze; GetZipItem(hz,-1,&ze); int numentries=ze.index;
  bool allok=true;
  for (int i=0; i<numentries && allok; i++)
  { GetZipItem(hz,i,&ze);
    string ext = StringLower(ExtractFileExt(ze.name));
    if (StringLower(ze.name)=="mainstick.txt")
    { char *buf=new char[ze.unc_size+1];
      UnzipItem(hz,i, buf,ze.unc_size);
      buf[ze.unc_size]=0;
      ReadDataFlags rflags=rdOverwrite;
      if (forediting) rflags=(ReadDataFlags)(rflags|rdStrict);
      allok &= newbody->ReadData(buf,err,rflags,NULL);
      delete[] buf;
    }
    else if (ext==".bmp" || ext==".jpg" || ext==".jpeg")
    { TBmp bmp;
      bmp.buf = new char[ze.unc_size];
      bmp.bufsize = ze.unc_size;
      bmp.name = ChangeFileExt(ze.name,"");
      UnzipItem(hz,i, bmp.buf,bmp.bufsize);
      PrepareBitmapData(&bmp,usedibsections);
      if (!forediting) {delete[] bmp.buf; bmp.buf=0; bmp.bufsize=0;}
      newbody->bmps.push_back(bmp);
    }
    else if (StringLower(ze.name)=="styles.txt")
    { if (forediting)
      { char *buf = new char[ze.unc_size+1];
        UnzipItem(hz,i, buf,ze.unc_size);
        buf[ze.unc_size]=0;
        allok &= StylesFromString(buf,newbody->styles);
        delete[] buf;
      }
    }
    // we just ignore anything we don't yet know:
  }
  CloseZip(hz);
  if (!allok) {delete newbody; return false;}
  delete body; body=newbody; *pbody=newbody;
  // fix up bitmap,effect information
  MakeBindexes(body);
  MakeEindexes(body);
  return true;
}

bool LoadBody(TBody **pbody,const char *fnsrc, char *err,LoadBodyFlags flags)
{ HANDLE hFile=CreateFile(fnsrc,GENERIC_READ,FILE_SHARE_READ,NULL,OPEN_EXISTING,0,NULL);
  if (hFile==INVALID_HANDLE_VALUE) {wsprintf(err,"Unable to open file %s",fnsrc); return false;}
  BY_HANDLE_FILE_INFORMATION finfo; GetFileInformationByHandle(hFile,&finfo);
  if (finfo.nFileSizeLow==0) {wsprintf(err,"File is empty %s",fnsrc); return false;}
  char *buf = new char[finfo.nFileSizeLow+1];
  DWORD red; ReadFile(hFile,buf,finfo.nFileSizeLow,&red,NULL);
  CloseHandle(hFile);
  buf[finfo.nFileSizeLow]=0;
  bool res = LoadBodyMem(pbody,buf,finfo.nFileSizeLow,err,flags);
  delete[] buf;
  return res;
}
bool LoadBodyZip(TBody **pbody,void *hzsrc,int indexsrc, char *err,LoadBodyFlags flags)
{ ZIPENTRY ze; GetZipItem((HZIP)hzsrc,indexsrc,&ze);
  vector<char> buf(ze.unc_size+1);
  UnzipItem((HZIP)hzsrc,indexsrc,&buf[0],ze.unc_size);
  buf[ze.unc_size]=0;
  return LoadBodyMem(pbody,&buf[0],ze.unc_size,err,flags);
}






inline int add_pt(b2func pb2x,b2func pb2y,void *dat,TEffPt *ept,int ptpos,double x,double y,int id)
{ // the coordinates are in body coordinates
  int xx=pb2x(dat,x), yy=pb2y(dat,y);
  return ept->add(ptpos,xx,yy,id);
}
inline int add_pt(b2func pb2x,b2func pb2y,void *dat,TEffPt *ept,int ptpos,TLimb &limb,bool ar,int id)
{ // ar specifies the arcroot of this limb
  double x,y; jointpos(&x,&y,limb,ar);
  return add_pt(pb2x,pb2y,dat,ept,ptpos,x,y,id);
}
int add_arc(b2func pb2x,b2func pb2y,void *dat,TEffPt *ept,int ptpos,TLimb &limb,bool forwards,int id)
{ // forwards goes from the arcroot to the joint; reverse the opposite.
  // The arc doesn't quite touch the final point.
  if (limb.type!=1 && limb.type!=3) return add_pt(pb2x,pb2y,dat,ept,ptpos,limb,forwards,id);
  double arange = limb.ang-limb.ang0; if (arange<0) arange=-arange;
  if (limb.type==3) arange=1.96*pi;
  double perim = arange*limb.length; perim = (abs(pb2x(dat,perim))+abs(pb2y(dat,perim)))/2; // in pixels
  double dnumdivs=1+arange*12/pi;
  double mul=1+log(perim/50)/log(2); if (mul<1) mul=1;
  dnumdivs*=mul; int numdivs=(int)dnumdivs;
  if (numdivs>100) numdivs=100;
  ept->ensure(ptpos+numdivs);
  for (int fi=0; fi<numdivs; fi++)
  { double f = ((double)fi)/((double)(numdivs));
    if (!forwards) f=1.0-f;
    double x,y; jointpos(&x,&y,limb,true,f);
    ept->pt[ptpos].x=pb2x(dat,x); ept->pt[ptpos].y=pb2y(dat,y); ept->pi[ptpos]=id;
    ptpos++;
  }
  return ptpos;
}




typedef struct {double anchx;double anchy;double scale;int soffx;int soffy;} Dat2b;

int g2b2x(void *dat,double x)
{ Dat2b *bd = (Dat2b*)dat; if (bd==0) return 0;
  return (int)((x+bd->anchx+5)*bd->scale+0.5)+bd->soffx;
}
int g2b2y(void *dat,double y)
{ Dat2b *bd = (Dat2b*)dat; if (bd==0) return 0;
  return (int)((y+bd->anchy+5)*bd->scale+0.5)+bd->soffy;
}

class TSimpleAutoPen
{ public:
  HPEN hpen, hprev;
  bool set(double scaledthickness,int r,int g,int b)
  { if (r==-1) {r=GetRValue(adef); g=GetGValue(adef); b=GetBValue(adef);}
    if (scaledthickness==athick && r==ar && g==ag && b==ab && hpen!=0) return false;
    if (hpen!=0)
    { if (hprev!=0) {DeleteObject(hprev);}
      hprev=hpen;
    }
    double width = LineThickness*scaledthickness;
    int iwidth = (int)(width+0.5);
    hpen = CreatePen(PS_SOLID, iwidth, RGB(r,g,b));
    athick=scaledthickness; ar=r; ag=g; ab=b;
    return true;
  }
  TSimpleAutoPen(COLORREF def) {hpen=0; hprev=0; adef=def;}
  ~TSimpleAutoPen()
  { if (hprev!=0) {DeleteObject(hprev); hprev=0;}
    if (hpen!=0)  {DeleteObject(hpen);  hpen=0;}
  }
protected:
  double athick; int ar,ag,ab; COLORREF adef;
};


void bmp_cache(HDC sdc, TBmp &bmp, int w, int h)
{ if (bmp.hbcache!=0 && bmp.cwidth==w && bmp.cheight==h) return;
  if (bmp.hbm==0) return;
  if (bmp.hbcache!=0) DeleteObject(bmp.hbcache); bmp.hbcache=0;
  if (bmp.hbmaskcache!=0) DeleteObject(bmp.hbmaskcache); bmp.hbmaskcache=0;
  bmp.cprevi = (bmp.cprevi+1)%5;
  bmp.cprev[bmp.cprevi].cx=w; bmp.cprev[bmp.cprevi].cy=h;
  for (int i=0; i<5; i++) {if (bmp.cprev[i].cx!=w || bmp.cprev[i].cy!=h) return;}
  // otherwise, it looks like we've settled upon a size for the bitmap
  HDC hdcSrc=CreateCompatibleDC(sdc), hdcDst=CreateCompatibleDC(sdc);
  bmp.hbcache=CreateCompatibleBitmap(sdc,w,h);
  bmp.cwidth=w; bmp.cheight=h;
  SelectObject(hdcSrc,bmp.hbm);
  SelectObject(hdcDst,bmp.hbcache);
  SetStretchBltMode(hdcDst,COLORONCOLOR);
  StretchBlt(hdcDst,0,0,w,h,hdcSrc,0,0,bmp.bwidth,bmp.bheight,SRCCOPY);
  //
  // it turned out that my attempt to cache the mask had no effect
  if (bmp.hbmask!=0)
  { //bmp.hbmaskcache=CreateCompatibleBitmap(sdc,w,h);
    //SelectObject(hdcSrc,bmp.hbmask);
    //SelectObject(hdcDst,bmp.hbmaskcache);
    //SetStretchBltMode(hdcDst,COLORONCOLOR);
    //StretchBlt(hdcDst,0,0,w,h,hdcSrc,0,0,bmp.bwidth,bmp.bheight,SRCCOPY);
  }
  //
  DeleteDC(hdcSrc); DeleteDC(hdcDst);
}


bool SimpleDraw(HDC hdc,RECT &rc,TBody *body,const char *ban,TAmpData *ad,string *err)
{ bool fail=false;
  // The body has dimensions -5 to +5, and we add anchx/anchy to the body's coordinates.
  // This 10x10 square must be mapped onto the largest square inside rc.
  // We do this as screenx = (bodyx+anchx+5)*scale+soffset
  if (body!=0)
  {
  COLORREF clrForeground=RGB(255,255,255);
  COLORREF clrBackground = RGB(body->limbs[0].color.rgb.r, body->limbs[0].color.rgb.g, body->limbs[0].color.rgb.b);
  double scale; int soffx,soffy;
  int width=rc.right-rc.left, height=rc.bottom-rc.top;
  if (width>height) {soffx=rc.left+(width-height)/2; soffy=rc.top+0; scale=((double)height)/10.0;}
  else {soffx=rc.left+0; soffy=rc.top+(height-width)/2; scale=width/10;}
  Dat2b dat; dat.anchx=body->anchx; dat.anchy=body->anchy; dat.scale=scale; dat.soffx=soffx; dat.soffy=soffy;
  //
  TEffPt ept;
  HBRUSH hbr = CreateSolidBrush(clrBackground); 
  TSimpleAutoPen lpen(clrForeground); lpen.set(0,-1,-1,-1);
  HGDIOBJ holdbr = SelectObject(hdc,hbr); 
  HGDIOBJ holdpen = SelectObject(hdc,lpen.hpen); 
  SetGraphicsMode(hdc,GM_ADVANCED);
  SetStretchBltMode(hdc,COLORONCOLOR);
  SetBkMode(hdc,clrBackground);
  HDC tdc=CreateCompatibleDC(hdc); HGDIOBJ holdt=0; // for any bitmaps we might draw
  FillRect(hdc,&rc,hbr);
  //
  for (int s=0; s<(int)body->shapes.size(); s++)
  { TShape &shape = body->shapes[s];
    if (!shape.limbs)
    { SetPolyFillMode(hdc,shape.balternate?ALTERNATE:WINDING);
      bool simplecircle = (shape.p.size()==2 && shape.p[0].i==shape.p[1].i && shape.p[0].ar!=shape.p[1].ar && body->limbs[shape.p[0].i].type==3);
      if (shape.brush.dtype==ctBitmap && (shape.brush.bindex<0 || shape.brush.bindex>=(int)body->bmps.size())) {fail=true; if (err!=0) *err="bindex";}
      if (simplecircle && shape.brush.dtype==ctBitmap && shape.brush.bindex!=-1)
      { TLimb &limb = body->limbs[shape.p[0].i];
        double dx=limb.x-limb.x0,dy=limb.y-limb.y0, rad=sqrt(dx*dx+dy*dy);
        int x0=g2b2x(&dat,limb.x0-rad), y0=g2b2y(&dat,limb.y0-rad), x1=g2b2x(&dat,limb.x0+rad), y1=g2b2y(&dat,limb.y0+rad);
        TBmp &bmp = body->bmps[shape.brush.bindex];
        // adjust the rect so the bitmap is kept proportional
        double f=((double)bmp.bwidth)/((double)bmp.bheight);
        if (f>1) {int cy=(y0+y1)/2, h=y1-y0; h=(int)(((double)h)/f); y0=cy-h/2; y1=cy+h/2;}
        else if (f<1) {int cx=(x0+x1)/2, w=x1-x0; w=(int)(((double)w)*f); x0=cx-w/2; x1=cx+w/2;}
        XFORM xf; FLOAT ang=(FLOAT)(limb.ang);
        FLOAT c=(FLOAT)cos(ang), s=(FLOAT)sin(ang);
        FLOAT cx=(FLOAT)(x0+(x1-x0)/2), cy=(FLOAT)(y0+(y1-y0)/2);
        xf.eM11=c; xf.eM12=s; xf.eM21=-s; xf.eM22=c;
        xf.eDx=cx+cy*s-cx*c;  xf.eDy=cy-cy*c-cx*s;
        if (ang!=0) SetWorldTransform(hdc,&xf);
        if (limb.chan==4 && limb.band==0 && f==0) bmp_cache(hdc,bmp,x1-x0,y1-y0);
        //
        if (bmp.hbmask!=0)
        { HBITMAP m=bmp.hbmaskcache; if (m==0) m=bmp.hbmask;
          HGDIOBJ o=SelectObject(tdc,m); if (holdt==0) holdt=o;
          if (m==bmp.hbmaskcache) BitBlt(hdc,x0,y0,x1-x0,y1-y0,tdc,0,0,SRCAND);
          else StretchBlt(hdc,x0,y0,x1-x0,y1-y0,tdc,0,0,bmp.bwidth,bmp.bheight,SRCAND);
        }
        //
        if (bmp.hbm!=0)
        { HBITMAP m=bmp.hbcache; if (m==0) m=bmp.hbm;
          HGDIOBJ o=SelectObject(tdc,m); if (holdt==0) holdt=o;
          DWORD rop=SRCCOPY; if (bmp.hbmask!=0) rop=SRCPAINT;
          if (m==bmp.hbcache) BitBlt(hdc,x0,y0,x1-x0,y1-y0,tdc,0,0,rop);
          else StretchBlt(hdc,x0,y0,x1-x0,y1-y0,tdc,0,0,bmp.bwidth,bmp.bheight,rop);
        }
        //
        xf.eM11=1; xf.eM12=0; xf.eM21=0; xf.eM22=1; xf.eDx=0; xf.eDy=0;
        if (ang!=0) SetWorldTransform(hdc,&xf);
      }
      if (simplecircle)
      { TLimb &limb = body->limbs[shape.p[0].i];
        double dx=limb.x-limb.x0,dy=limb.y-limb.y0, rad=sqrt(dx*dx+dy*dy);
        int x0=g2b2x(&dat,limb.x0-rad), y0=g2b2y(&dat,limb.y0-rad), x1=g2b2x(&dat,limb.x0+rad), y1=g2b2y(&dat,limb.y0+rad);
        HPEN hspen=CreatePen(PS_NULL,0,0); HGDIOBJ hop=SelectObject(hdc,hspen);
        if (shape.line.dtype==ctNone) {}
        else if (shape.line.dtype==ctRGB)
        { lpen.set(scale*shape.thickness,shape.line.rgb.r,shape.line.rgb.g,shape.line.rgb.b);
          SelectObject(hdc,lpen.hpen);
        }
        else {fail=true; if (err!=0) *err="shape line colour";}
        //
        LOGBRUSH lbr; lbr.lbStyle=BS_SOLID; if (shape.brush.dtype==ctNone) lbr.lbStyle=BS_NULL;
        else if (shape.brush.dtype==ctDefault) lbr.lbColor=clrForeground;
		else if (shape.brush.dtype == ctRGB) lbr.lbColor = RGB(shape.brush.rgb.r, shape.brush.rgb.g, shape.brush.rgb.b);
        else if (shape.brush.dtype==ctBitmap) lbr.lbStyle=BS_NULL;
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
          TLimb &limb0=body->limbs[j0.i]; //&limb1=body->limbs[j1.i];
          if (j0.i==j1.i && j0.ar!=j1.ar && (limb0.type==1 || limb0.type==3)) ptpos=add_arc(g2b2x,g2b2y,&dat,&ept,ptpos,limb0,j0.ar,-1);
          else ptpos=add_pt(g2b2x,g2b2y,&dat,&ept,ptpos,limb0,j0.ar,-1);
        }
        if (shape.p.size()>2) ptpos=ept.add(ptpos,ept.pt[0].x,ept.pt[0].y,(int)shape.p.size()-1); // to close it!      
        if (shape.brush.dtype!=ctNone)
        { HPEN hspen=CreatePen(PS_NULL,0,0); HGDIOBJ hop = SelectObject(hdc,hspen);
          LOGBRUSH lbr; lbr.lbStyle=BS_SOLID;
          if (shape.brush.dtype==ctDefault) lbr.lbColor=clrForeground;
		  else if (shape.brush.dtype == ctRGB) lbr.lbColor = RGB(shape.brush.rgb.r, shape.brush.rgb.g, shape.brush.rgb.b);
          else {fail=true; if (err!=0) *err="brush colour";}
          HBRUSH hsbr=CreateBrushIndirect(&lbr); HGDIOBJ hob = SelectObject(hdc,hsbr);
          Polygon(hdc,ept.pt,ptpos-1);
          SelectObject(hdc,hob); DeleteObject(hsbr);
          SelectObject(hdc,hop); DeleteObject(hspen);
        }
        if (shape.line.dtype!=ctNone)
        { if (shape.line.dtype!=ctRGB) {fail=true; if (err!=0) *err="shape line colour";}
          lpen.set(scale*shape.thickness,shape.line.rgb.r,shape.line.rgb.g,shape.line.rgb.b);
          SelectObject(hdc,lpen.hpen);
          Polyline(hdc,ept.pt,ptpos);
        }
      }
    }
    else // if shape.limbs
    { SelectObject(hdc,GetStockObject(NULL_BRUSH));
      for (int n=1; n<body->nlimbs; n++)
      { TLimb &limb=body->limbs[n];
        int lx0 = (int)((limb.x0+body->anchx+5)*scale+0.5)+soffx;
        int ly0 = (int)((limb.y0+body->anchy+5)*scale+0.5)+soffy;
        int lx1 = (int)((limb.x +body->anchx+5)*scale+0.5)+soffx;
        int ly1 = (int)((limb.y +body->anchy+5)*scale+0.5)+soffy;
        if (limb.color.dtype!=ctNone)
        { if (limb.color.dtype!=ctRGB && limb.color.dtype!=ctDefault) {fail=true; if (err!=0) *err="line colour";}
		int r = limb.color.rgb.r, g = limb.color.rgb.g, b = limb.color.rgb.b;
          if (limb.color.dtype==ctDefault) {r=-1; g=-1; b=-1;}
          bool changed = lpen.set(scale*limb.thickness,r,g,b);
          if (changed) {SelectObject(hdc,lpen.hpen); }
          if (limb.type==0 || limb.type==2) // a line
          { MoveToEx(hdc,lx0,ly0,NULL);
            LineTo(hdc,lx1,ly1);
          }
          else if (limb.type==1) // an arc. Origin at limb->x0y0. Radius=limb->length. Ends at limb->ang0,1
          { double d=scale*limb.length;
            double angA,angB; if (limb.ascale<0) {angA=limb.ang0;angB=limb.ang;} else {angA=limb.ang; angB=limb.ang0;}
            int xA=lx0+(int)(d*cos(angA)), yA=ly0+(int)(d*sin(angA));
            int xB=lx0+(int)(d*cos(angB)), yB=ly0+(int)(d*sin(angB));
            int ddAB=(xB-xA)*(xB-xA)+(yB-yA)*(yB-yA);
            if (ddAB<100 && angA-angB<pi) {MoveToEx(hdc,xA,yA,NULL); LineTo(hdc,xB,yB);} // draw a line for things less than ten pixels
            else Arc(hdc,(int)(lx0-d),(int)(ly0-d),(int)(lx0+d),(int)(ly0+d), xA,yA, xB,yB);
          }
          else if (limb.type==3) // a circle. Origin at limb->x0y0. Radius according to limb.x,limb.y
          { double dx=limb.x-limb.x0,dy=limb.y-limb.y0, d=scale*sqrt(dx*dx+dy*dy); int id=(int)d;
            Ellipse(hdc,lx0-id,ly0-id,lx0+id,ly0+id);
          }
        }
      }
    }
  }
  //
  SelectObject(hdc,holdbr);
  DeleteObject(hbr); 
  SelectObject(hdc,holdpen);
  if (holdt!=0) SelectObject(tdc,holdt); DeleteDC(tdc);
  }

  //

  if (ad!=0)
  { HPEN hp = CreatePen(PS_SOLID,2,RGB(255,0,0));
    HGDIOBJ hop=SelectObject(hdc,hp);
    // draw bars for left/right channels
    for (int p=0; p<6; p++)
    { RECT prc=rc;
      int x0=rc.left+p*10, x1=rc.left+p*10+9, x2=rc.right-p*10-9, x3=rc.right-p*10;
      RECT src; src.top=rc.bottom-1-(int)(ad->max[p]/10); src.right=x1;
      src.left=x0; src.bottom=rc.bottom-1-(int)(ad->min[p]/10);
      FillRect(hdc,&src,(HBRUSH)GetStockObject(WHITE_BRUSH));
      src.top=prc.bottom-1-(int)(ad->max[p]/10); src.right=x3;
      src.left=x2; src.bottom=rc.bottom-1-(int)(ad->min[p]/10);
      FillRect(hdc,&src,(HBRUSH)GetStockObject(WHITE_BRUSH));
      MoveToEx(hdc,x0,rc.bottom-1-(int)((ad->min[p]+ad->l[p]*(ad->max[p]-ad->min[p]))/10),NULL);
      LineTo(hdc,x1,rc.bottom-1-(int)((ad->min[p]+ad->l[p]*(ad->max[p]-ad->min[p]))/10));
      MoveToEx(hdc,x2,rc.bottom-1-(int)((ad->min[p]+ad->r[p]*(ad->max[p]-ad->min[p]))/10),NULL);
      LineTo(hdc,x3,rc.bottom-1-(int)((ad->min[p]+ad->r[p]*(ad->max[p]-ad->min[p]))/10));
    }
    // draw karaoke bars (0.vocals on left, 1.music on right)
    int x0=rc.left+7*10, x1=rc.left+7*10+9, x2=rc.left+8*10, x3=rc.left+8*10+9;
    RECT src; src.top=rc.bottom-1-(int)(ad->kmax[0]/10); src.right=x1;
    src.left=x0; src.bottom=rc.bottom-1-(int)(ad->kmin[0]/10);
    FillRect(hdc,&src,(HBRUSH)GetStockObject(WHITE_BRUSH));
    MoveToEx(hdc,x0,rc.bottom-1-(int)((ad->kmin[0]+ad->k[0]*(ad->kmax[0]-ad->kmin[0]))/10),NULL);
    LineTo(hdc,x1,rc.bottom-1-(int)((ad->kmin[0]+ad->k[0]*(ad->kmax[0]-ad->kmin[0]))/10));
    //
    src.top=rc.bottom-1-(int)(ad->kmax[1]/10); src.right=x3;
    src.left=x2; src.bottom=rc.bottom-1-(int)(ad->kmin[1]/10);
    FillRect(hdc,&src,(HBRUSH)GetStockObject(WHITE_BRUSH));
    MoveToEx(hdc,x2,rc.bottom-1-(int)((ad->kmin[1]+ad->k[1]*(ad->kmax[1]-ad->kmin[1]))/10),NULL);
    LineTo(hdc,x3,rc.bottom-1-(int)((ad->kmin[1]+ad->k[1]*(ad->kmax[1]-ad->kmin[1]))/10));
    //
    SelectObject(hdc,hop);
    DeleteObject(hp);
  }

  if (ban!=0 && *ban!=0)
  { RECT brc;
    brc.right = rc.right*10/12;
    brc.left = rc.right*2/12;
    brc.top = rc.bottom*17/20;
    brc.bottom = rc.bottom;
    FitText(hdc,rc.right/20,&brc,ban);
  }

  return !fail;

}



void FitText(HDC hdc,int defheight, RECT *rc,const string t)
{ LOGFONT lf; ZeroMemory(&lf,sizeof(lf));
  lf.lfHeight = defheight;
  lf.lfWidth=0;
  lf.lfEscapement=0;
  lf.lfOrientation=0;
  lf.lfWeight=FW_BOLD;
  lf.lfItalic=FALSE;
  lf.lfUnderline=FALSE;
  lf.lfStrikeOut=FALSE;
  lf.lfCharSet=DEFAULT_CHARSET;
  lf.lfOutPrecision=OUT_DEFAULT_PRECIS;
  lf.lfClipPrecision=CLIP_DEFAULT_PRECIS;
  lf.lfQuality=DEFAULT_QUALITY;
  lf.lfPitchAndFamily=VARIABLE_PITCH|FF_SWISS;
  strcpy(lf.lfFaceName,"");
  HFONT hf = CreateFontIndirect(&lf);
  HFONT holdf = (HFONT)SelectObject(hdc,hf);
  // split the thing up into multiple lines
  list<string> ts; string s=t; while (s.find("\r\n")!=string::npos)
  { size_t i = s.find("\r\n"); 
    ts.push_back(s.substr(0,i));
    s=s.substr(i+2);
  }
  ts.push_back(s);
  int maxwidth=0,maxheight=0;
  for (;;)
  { maxwidth=0;
    for (list<string>::const_iterator tsi=ts.begin(); tsi!=ts.end(); tsi++)
    { RECT trc; trc.left=0; trc.top=0; trc.right=0; trc.bottom=0;
      DrawText(hdc,tsi->c_str(),(int)tsi->length(),&trc,DT_CALCRECT|DT_SINGLELINE);
      int width=trc.right; if (width>maxwidth) maxwidth=width;
    }
    RECT trc; trc.left=0; trc.top=0; trc.right=maxwidth; trc.bottom=0;
    DrawText(hdc,t.c_str(),(int)t.length(),&trc,DT_CALCRECT);
    maxheight=trc.bottom;
    if (maxwidth<=rc->right-rc->left && maxheight<=rc->bottom-rc->top) break;
    SelectObject(hdc,holdf);
    DeleteObject(hf);
    if (lf.lfHeight<=6) return;
    int oh=lf.lfHeight; lf.lfHeight = lf.lfHeight*9/10; if (lf.lfHeight==oh) lf.lfHeight--;
    hf = CreateFontIndirect(&lf);
    holdf = (HFONT)SelectObject(hdc,hf);
  }
  SetBkMode(hdc,TRANSPARENT);
  int avx=(rc->right+rc->left)/2, avy=(rc->bottom+rc->top)/2;
  RECT trc; trc.left=avx-maxwidth/2; trc.right=avx+maxwidth/2; trc.top=avy-maxheight/2; trc.bottom=avy+maxheight/2;
  //
  SetTextColor(hdc,RGB(0,0,0));
  DrawText(hdc,t.c_str(),(int)t.length(),&trc,DT_CENTER|DT_NOCLIP);
  OffsetRect(&trc,1,1);
  SetTextColor(hdc,RGB(255,255,255));
  DrawText(hdc,t.c_str(),(int)t.length(),&trc,DT_CENTER|DT_NOCLIP);
  //
  SelectObject(hdc,holdf);
  DeleteObject(hf);
}







int hasbeendealtwith=-2;
void HideRedundancy(vector<TPre> &pre)
{ if (hasbeendealtwith==1) return;
  if (hasbeendealtwith==-2)
  { hasbeendealtwith=0;
    HKEY key; LONG res=RegOpenKeyEx(HKEY_CURRENT_USER,"Software\\Lu\\Sticky",0,KEY_READ,&key);
    if (res==ERROR_SUCCESS)
    { DWORD dat, type, size=sizeof(dat);
      res=RegQueryValueEx(key,"DealtWithRedundancy3.4",NULL,&type,(LPBYTE)&dat,&size);
      if (res==ERROR_SUCCESS) hasbeendealtwith=1;
      RegCloseKey(key);
    }
    if (hasbeendealtwith==1) return;
  }
  // did we encounter any redundancy?
  bool hasold=false, hasnew=false;
  for (vector<TPre>::const_iterator i=pre.begin(); i!=pre.end() && !(hasold&&hasnew); i++)
  { if (StringLower(i->desc)=="dancer (vale)") hasold=true;
    if (StringLower(i->desc)=="dancers\\dancer (vale)") hasnew=true;
  }
  if (!hasold || !hasnew) return;
  //
  // alright, there is redundancy, and it hasn't been dealt with, so we will remove the
  // redundant things from our list. But we'll leave it to the editor to actually
  // remove the redundancy.
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
  for (int i=0; i<(int)pre.size(); i++)
  { string s = StringLower(pre[i].desc);
    const char *entry = s.c_str();
    for (int j=0; j<nos; j++)
    { const char *c = strstr(entry,oss[j]);
      if (c==entry) olds[j]=i; else if (c!=0) news[j]=i;
    }
  }
  // now remove them!
  for (int i=0; i<nos; i++)
  { if (olds[i]!=-1 && news[i]!=-1 && olds[i]!=news[i])
    { int op=olds[i];
      pre.erase(pre.begin()+op);
      for (int j=i+1; j<nos; j++) {if (olds[j]>op) olds[j]--; if (news[j]>op) news[j]--;}
    }
  }
}





// EnsureSubmenu -- for a given name, either returns a submenu with the name (if it exists)
// or creates a submenu with the name and returns that.
HMENU EnsureSubmenu(HMENU hmenu,const string find)
{ int n=GetMenuItemCount(hmenu); MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi);
  char c[MAX_PATH]; mi.fMask=MIIM_TYPE|MIIM_SUBMENU; mi.dwTypeData=c; mi.cch=MAX_PATH;
  for (int i=0; i<n; i++)
  { BOOL res=GetMenuItemInfo(hmenu,i,TRUE,&mi);
    string s(c);
    if (res && StringLower(s)==StringLower(find) && mi.hSubMenu!=NULL) return mi.hSubMenu;
  }
  // otherwise it wasn't found, so we'll create it
  HMENU hm=CreatePopupMenu();
  mi.fMask=MIIM_DATA|MIIM_ID|MIIM_STATE|MIIM_TYPE|MIIM_SUBMENU;
  mi.hSubMenu=hm; mi.fType=MFT_STRING; mi.fState=MFS_ENABLED;
  mi.dwTypeData=(LPSTR)find.c_str(); mi.cch=(int)find.length(); InsertMenuItem(hmenu,0,TRUE,&mi);
  return hm;
}


// PopulateMenu -- for a given name/id, adds it to the menu. But if the name
// was prefixed with directory-separators, like fred\blogs\file, then we put it
// in submenus.
void PopulateMenu(HMENU hmenu,const char *desc,int id,bool checked)
{ MENUITEMINFO mi; ZeroMemory(&mi,sizeof(mi)); mi.cbSize=sizeof(mi);
  mi.fMask=MIIM_TYPE|MIIM_ID|MIIM_STATE; mi.fType=MFT_STRING; mi.fState=MFS_ENABLED;
  if (checked) mi.fState |= MFS_CHECKED;
  HMENU hthismenu=hmenu; const char *c=desc;
  while (strchr(c,'\\')!=NULL)
  { const char *subpos=strchr(c,'\\');
    hthismenu=EnsureSubmenu(hthismenu, string(c,subpos-c));
    c=subpos+1;
  }
  mi.wID=id; mi.dwTypeData=(LPSTR)c; mi.cch=(int)strlen(desc);
  InsertMenuItem(hthismenu,0,TRUE,&mi);
}


} // namespace

