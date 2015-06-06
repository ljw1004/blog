#include <windows.h>
#include <string>
#include <list>
#include <iostream>
using namespace std;
#include "../zip.h"


// This program does a recursive directory sweep.
// Any directory it finds with (..) at the end of its name,
// so long as that directory has no subdirectories,
// it creates a ".sticks" file out of it


bool GetSuspect(const string prefix, list<string> *fns)
{ string match = prefix+"*";
  WIN32_FIND_DATA fdat; HANDLE hfind=FindFirstFile(match.c_str(),&fdat);
  for (BOOL res=(hfind!=INVALID_HANDLE_VALUE); res; res=FindNextFile(hfind,&fdat))
  { if (strcmp(fdat.cFileName,".")==0 || strcmp(fdat.cFileName,"..")==0) continue;
    if ((fdat.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)!=0) return false;
    fns->push_back(prefix+fdat.cFileName);
  }
  if (hfind!=INVALID_HANDLE_VALUE) FindClose(hfind);
  return (fns->size()>0);
}
    
char prefix[257]="STKZ - a .sticks zip file. This header is so that\r\n"
                 "IE allows the file to be opened.\r\n\r\n"
                 "I give thanks that I am fearfully, wonderfully made.\r\n\r\n"
                 "You have taken note of him.\r\n"
                 "You have made him little less than divine\r\n"
                 "and adorned him in glory and majesty.\r\n\r\n";

string ExtractFileName(const string fn)
{ const char *c=fn.c_str(), *lastslash=c;
  while (*c!=0) {if (*c=='\\' || *c=='/') lastslash=c+1; c++;}
  return lastslash;
}


bool MakeSticks(const string dstfn, const list<string> srcfns)
{ bool exists = (GetFileAttributes(dstfn.c_str())!=0xFFFFFFFF);
  HANDLE hf = CreateFile(dstfn.c_str(),GENERIC_WRITE,0,NULL,CREATE_ALWAYS,FILE_ATTRIBUTE_NORMAL,0);
  if (hf==INVALID_HANDLE_VALUE) {cout << "Error creating '" << dstfn << "'" << endl; return false;}
  DWORD writ; WriteFile(hf,prefix,256,&writ,0);
  HZIP hzip = CreateZip(hf,0,ZIP_HANDLE);
  for (list<string>::const_iterator i=srcfns.begin(); i!=srcfns.end(); i++)
  { string fname=ExtractFileName(*i);
    ZipAdd(hzip,fname.c_str(),(void*)i->c_str(),0,ZIP_FILENAME);
  }
  CloseZip(hzip);
  CloseHandle(hf);
  if (exists) cout << "Replaced '"; else cout << "Created '";
  cout << dstfn << "' (" << (int)srcfns.size() << " sticks)" << endl;
  return true;
}


void Rec(const string prefix)
{ // prefix is "dir\\" or "dir\\subdir\\"
  string match = prefix+"*";
  WIN32_FIND_DATA fdat; HANDLE hfind=FindFirstFile(match.c_str(),&fdat);
  for (BOOL res=(hfind!=INVALID_HANDLE_VALUE); res; res=FindNextFile(hfind,&fdat))
  { if (strcmp(fdat.cFileName,".")==0 || strcmp(fdat.cFileName,"..")==0) continue;
    if ((fdat.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)==0) continue;
    string dir=prefix+fdat.cFileName;
    if (fdat.cFileName[strlen(fdat.cFileName)-1]!=')') {Rec(dir+"\\"); continue;}
    list<string> fns; bool ok=GetSuspect(dir+"\\",&fns); if (!ok) continue;
    ok=MakeSticks(dir+".sticks",fns); if (!ok) continue;
    for (list<string>::const_iterator i=fns.begin(); i!=fns.end(); i++) DeleteFile(i->c_str());
    RemoveDirectory(dir.c_str());
  }
  if (hfind!=INVALID_HANDLE_VALUE) FindClose(hfind);
}

void main()
{ cout << "Autozip - a recursive directory scan to turn named leaf directories into .sticks" << endl;
  Rec("");
}