#pragma warning( push )
#pragma warning( disable: 4786 4702 )
#include <string>
#include <list>
#include <vector>
#include <algorithm>
#include <iostream>
#include <fstream>
using namespace std;
#pragma warning( pop )
#include <stdio.h>
#include <time.h>


// This program takes the stick-file-database "extradb.txt"
// and emits html for it to "extrastics-<cat>.html"


struct dbitem
{ string fn; string cat; string back; string by; bool multiple;
  bool operator<(const dbitem &b) const
  { if (cat<b.cat) return true;
    if (cat>b.cat) return false;
    if (fn<b.fn) return true;
    return false;
  }
};

struct catitem
{ string cat;      // the name of just this category on its own
  string fn;       // a filename for it, without the "extrasticks-" prefix or the ".html" suffix
  string title;    // a title for it
  int allsubitems; // count of all subitems, both immediate and descendent
  list<int> items;
  list<int> dirs;
};

struct pageitem
{ int dir;      // index into 'catitem'
  string name;  // the name that appears in the list
  string sep;   // separator that comes after it in the list
};

vector<dbitem> db;
vector<catitem> cats;
vector<pageitem> pages;






string htmlise(const string fn)
{ // change a filename into something that can be <a href="fn">'d
  vector<char> buf(fn.length()*4+1);
  const char *c=fn.c_str(); char *d=&buf[0];
  while (*c!=0)
  { if (*c==' ') {d[0]='%'; d[1]='2'; d[2]='0'; d+=3;}
    else {*d=*c; d++;}
    c++;
  }
  *d=0;
  return string(&buf[0]);
}

string nbspify(const string fn)
{ // turn all spaces into &nbsp;
  vector<char> buf(fn.length()*6+1);
  const char *c=fn.c_str(); char *d=&buf[0];
  while (*c!=0)
  { if (*c==' ') {d[0]='&'; d[1]='n'; d[2]='b'; d[3]='s'; d[4]='p'; d[5]=';'; d+=6;}
    else {*d=*c; d++;}
    c++;
  }
  *d=0;
  return string(&buf[0]);
}

string anonymise(const string fn)
{ // if something ends in " (stuff)", get rid of it
  char *buf = new char[fn.length()+1];
  const char *c=fn.c_str(); char *d=buf;
  while (*c!=0 && !(c[0]==' ' && c[1]=='(')) {*d=*c; c++; d++;}
  *d=0;
  string s(buf);
  delete[] buf;
  return s;
}
  

string changeext(const string fn,const string ext)
{ // given "file.stk" and ".gif", return "file.gif"
  int i=0; string Delimiters=".\\/:";
  if (fn!="")
  { for (int j=(int)fn.length()-1; j>=0; j--)
    { if (Delimiters.find(fn[j])!=string::npos) {i=j; break;}
    }
  }
  if (i==0 || fn[i]!='.') return fn+ext;
  return fn.substr(0,i)+ext;
}





int ensure_cat(const string path,int pos=0)
{ // c = "dir\dir\dir"
  const char *c=path.c_str(), *d=c;
  if (*c==0) return pos;
  while (*d!=0 && *d!='\\' && *d!='/') d++;
  const char *rest=d; if (*rest!=0) rest++;
  string name(c,d-c);
  catitem &cur = cats[pos];
  for (list<int>::const_iterator i=cur.dirs.begin(); i!=cur.dirs.end(); i++)
  { if (cats[*i].cat==name) return ensure_cat(rest,*i);
  }
  catitem ci; ci.cat=name;
  int cpos=(int)cats.size(); cats.push_back(ci);
  cats[pos].dirs.push_back(cpos);
  return ensure_cat(rest,cpos);
}

void entitle(int pos=0, const string fnprefix="", const string titleprefix="")
{ string s = cats[pos].cat;
  string fn = fnprefix; if (fn!="") fn+="-"; fn+=s;
  string title = titleprefix; if (title!="") title+=": "; title+=s;
  cats[pos].fn = fn;
  cats[pos].title = nbspify(title);
  for (list<int>::const_iterator i=cats[pos].dirs.begin(); i!=cats[pos].dirs.end(); i++)
  { entitle(*i,fn,title);
  }
}

int count(int pos=0)
{ catitem &c=cats[pos]; int tot=0;
  for (list<int>::const_iterator i=c.dirs.begin(); i!=c.dirs.end(); i++) tot+=count(*i);
  tot+=(int)c.items.size();
  c.allsubitems=tot; return tot;
}

void collapse_smalls(int pos=0)
{ catitem &c=cats[pos];
  for (list<int>::iterator i=c.dirs.begin(); i!=c.dirs.end(); )
  { collapse_smalls(*i); if (cats[*i].allsubitems>8) {i++; continue;}
    c.items.splice(c.items.end(), cats[*i].items);
    i=c.dirs.erase(i);
  }
}

void make_pages(int pos=0, const string nameprefix="", const string sep=" --- ")
{ catitem &c=cats[pos];
  string _prefix,_sep;
  if (c.items.size()>0)
  { pageitem pi; pi.dir=pos; 
    pi.name=nameprefix; if (nameprefix!="") pi.name+=":&nbsp;"; pi.name+=nbspify(c.cat); pi.sep=sep;
    pages.push_back(pi);
    _prefix=""; _sep=",&nbsp;";
  }
  else
  { _prefix=nameprefix; if (nameprefix!="") _prefix+=":&nbsp;"; _prefix+=nbspify(c.cat);
    _sep=",&nbsp;";
  }
  for (list<int>::const_iterator i=c.dirs.begin(); i!=c.dirs.end(); i++)
  { make_pages(*i,_prefix,_sep); _prefix="";
    if (pages.size()>0) pages.back().sep=sep;
  }
  if (pages.size()>0) pages.back().sep=sep;
  //
  if (pos==0 && pages.size()>0) pages.back().sep="";
}









int main(int argc, char *argv[])
{ string fndb="extradb.txt";
  if (argc==2) fndb=argv[1];

 
  FILE *f=fopen(fndb.c_str(),"rb");
  if (f==0)
  { cout << "cannot open " << fndb << endl;
    return 0;
  }
  fseek(f,0,SEEK_END); int size=(int)ftell(f); fseek(f,0,SEEK_SET);
  char *buf = new char[size+1];
  fread(buf,1,size,f); buf[size]=0;
  fclose(f);
  //
  const char *c=buf;
  while (*c!=0)
  { while (*c==' ' || *c=='\t') c++; if (*c=='\r') c++;
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
    it.multiple = (it.fn.length()>7 && it.fn.substr(it.fn.length()-7)==".sticks");
    db.push_back(it);
  }
  delete[] buf;

  //
  //
  //

  // Now we turn this linear list into a hierarchy and then another list
  sort(db.begin(),db.end());
  catitem root; root.cat=""; cats.push_back(root); int ccati=0;
  for (int i=0; i<(int)db.size(); i++)
  { const dbitem &it = db[i];
    if (it.cat!=cats[ccati].cat) ccati=ensure_cat(it.cat);
    catitem &ci = cats[ccati];
    ci.items.push_back(i);
  }
  entitle();
  count();
  collapse_smalls();  
  make_pages();


  //
  //
  //
  
  time_t t; time(&t); struct tm *today=localtime(&t);
  char tbuf[200]; sprintf(tbuf,"_tmp_%02i%02i'%02i%02i%02i",(int)today->tm_mday,(int)today->tm_mon+1,(int)today->tm_hour,(int)today->tm_min,(int)today->tm_sec);
  string tdirsuffix(tbuf);
  const char *uendl = "\n";
  ofstream cbat(changeext(fndb,".bat").c_str(),ios::out|ios::binary);
  cbat << "cd sticks" << uendl << uendl;
  

  for (vector<pageitem>::const_iterator pi=pages.begin(); pi!=pages.end(); pi++)
  { const pageitem &page = *pi;
    const catitem &dir = cats[page.dir];
    const string fnhtml = "extrasticks-"+dir.fn+".html";
    ofstream chtml(fnhtml.c_str());
    //
    chtml << "<html>" << endl << "<head>" << endl;
    chtml << "<title>Extra Sticks - " << dir.title << "</title>" << endl;
    chtml << "<link rel=stylesheet href=\"extra.css\" TYPE=\"text/css\" title=\"Extra\">" << endl;
    chtml << "</head>" << endl << "<body>" << endl << endl;
    chtml << "<div style=\"text-align: center;\">" << endl;
    chtml << "<div style=\"width: 620px; text-align: center;\">" << endl;
    chtml << endl << endl << endl << endl;
    chtml << "<h1>" << dir.title << "</h1>" << endl << endl;
    chtml << "<p style=\"font-size: smaller;\">" << endl;
    chtml << "<a href=\"extrasticks.html\">Recent submissions</a> -" << endl;
    chtml << "<a href=\"extrasticks.html#epicks\">Editor's picks</a><br>" << endl;
    //
    for (vector<pageitem>::const_iterator spi=pages.begin(); spi!=pages.end(); spi++)
    { const pageitem &spage = *spi;
      const catitem &sdir = cats[spage.dir];
      const string sfnhtml = "extrasticks-"+sdir.fn+".html";
      string cursel; if (spage.dir==page.dir) cursel="class=\"cursel\" ";
      chtml << "<a " << cursel << "href=\"" << htmlise(sfnhtml) << "\">" << spage.name << "</a>" << spage.sep;
    }
    chtml << "</p>" << endl << endl << endl;
    //
    //
    chtml << "<table><tr><td style=\"padding-top: 4ex;\">&nbsp;</td></tr><tr>" << endl;
    chtml << "<td colspan=4 class=\"download\">Download them all: ";
    chtml << "<a href=\"all/" << htmlise(dir.fn) << ".sticks\">" << dir.fn << ".sticks</a> </td></tr><tr>" << endl << endl;
    int pos=0;

    for (list<int>::const_iterator si=dir.items.begin(); si!=dir.items.end(); si++)
    { const dbitem &it = db[*si];
      chtml << "<td class=\"thumb\"><a href=\"sticks/" << htmlise(it.fn) << "\">";
      chtml << "<img width=128 height=128" << endl;
      if (it.back!="") chtml << "class=\"" << it.back << "\" ";
      chtml << "src=\"sticks/" << htmlise(changeext(it.fn,".gif")) << "\"><br>" << endl;
      chtml << anonymise(changeext(it.fn,"")) << "</a><br> " << it.by << "</td>" << endl << endl;
      pos++;
      if (pos==4) {pos=0; chtml << "</tr><tr>" << endl << endl;}
    }
  
    chtml << "</tr></table>" << endl << endl;
    chtml << "</div></div>" << endl;
    chtml << "</body>" << endl << "</html>" << endl;
    chtml.close();


    string tdir = dir.fn+tdirsuffix;
    cbat << "mkdir \"" << tdir << "\"" << uendl;
    cbat << "cd \"" << tdir << "\"" << uendl;
    //
    for (list<int>::const_iterator si=dir.items.begin(); si!=dir.items.end(); si++)
    { const dbitem &it = db[*si];
      if (it.multiple) cbat << "unzip \"../" << it.fn << "\"" << uendl;
      else cbat << "cp \"../" << it.fn << "\" ." << uendl;
    }
    cbat << "zip -9 \"" << dir.fn << ".zip\" *.stk" << uendl;
    cbat << "cat ../../header.txt \"" << dir.fn << ".zip\" > \"../../all/" << dir.fn << ".sticks\"" << uendl;
    cbat << "cd .." << uendl;
    cbat << "zip -A \"../all/" << dir.fn << ".sticks\"" << uendl;
    cbat << "rm -f -r \"" << tdir << "\"" << uendl << uendl;
  }


  //
  //
  //
  
  string dirfn="All";
  string tdir = dirfn+tdirsuffix;
  cbat << "mkdir \"" << tdir << "\"" << uendl;
  cbat << "cd \"" << tdir << "\"" << uendl;
  //
  for (vector<dbitem>::const_iterator i=db.begin(); i!=db.end(); i++)
  { const dbitem &it = *i;
    if (it.multiple) cbat << "unzip \"../" << it.fn << "\"" << uendl;
    else cbat << "cp \"../" << it.fn << "\" ." << uendl;
  }
  cbat << "zip -9 \"" << dirfn << ".zip\" *.stk" << uendl;
  cbat << "cat ../../header.txt \"" << dirfn << ".zip\" > \"../../all/" << dirfn << ".sticks\"" << uendl;
  cbat << "cd .." << uendl;
  cbat << "zip -A \"../all/" << dirfn << ".sticks\"" << uendl;
  cbat << "rm -f -r \"" << tdir << "\"" << uendl << uendl;


  return 0;
}



