#ifndef ZIP_STD
#include <windows.h>
#endif
#include <stdio.h>
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
#include "unzip.h"

namespace stk {



int lustricmp(const TCHAR *s1, const TCHAR *s2)
{ unsigned char *us1=(unsigned char*)s1;
  unsigned char *us2=(unsigned char*)s2;
  for (; ; us1++, us2++)
  { int v1=tolower(*us1), v2=tolower(*us2);
    if (v1==v2) return 0;
    if (v1<v2) return -1;
    if (v1>v2) return 1;
  }
}



void jointpos(double *bx,double *by,const TLimb &limb,bool ar,double f)
{ // calculates the position of the joint of the limb. For normal
  // limbs, this is just limb.x and limb.y. But for arc-roots, it's
  // the start of the arc. Also, the optional argument f can find
  // positions part way through the arc.
  if (!ar) {*bx=limb.x; *by=limb.y; return;}
  if (limb.type==0 || limb.type==2) {*bx=limb.x; *by=limb.y; return;} // line or spring
  if (limb.type==1)
  { // arc: origin at limb.x0,y0. radius=limb.length. Ends at limb.ang0,1
    double d=limb.length;
    double angA,angB,ang,fi=1.0-f;
    if (limb.ascale<0) {angA=limb.ang0; angB=limb.ang; ang=angA*fi+angB*f;}
    else {angA=limb.ang; angB=limb.ang0; ang=angA*f+angB*fi;}
    *bx=limb.x0+d*cos(ang); *by=limb.y0+d*sin(ang);
  }
  else if (limb.type==3)
  { // circle: origin at limb.x0,y0. radius=calc'd from limb.xy.
    double dx=limb.x-limb.x0,dy=limb.y-limb.y0, d=sqrt(dx*dx+dy*dy);
    double angB=limb.ang,angA=angB-1.96*pi,fi=1.0-f, ang=angA*fi+angB*f;
    *bx=limb.x0+d*cos(ang); *by=limb.y0+d*sin(ang);
  }    
}


TStyle::TStyle() : name(""), shortcut(0)
{ limb.color.type=ctRGB; limb.color.rgb=RGBCOLOR(128,255,0); limb.thickness=1;
  limb.chan=0; limb.band=0; limb.negative=false;
  shape.brush.type=ctRGB; shape.brush.rgb=RGBCOLOR(255,128,0);
}

string TStyle::tostring() const
{ string s(""); char c[1000];
  sprintf(c,"%s=",name.c_str()); s+=string(c);
  if (shortcut!=0) {sprintf(c,"Ctrl+%c ",shortcut); s+=string(c);}
  bool limbvisible=(limb.color.type!=ctNone);
  sprintf(c,"line(%i,%i,%i,%i,%lg) ",(limbvisible?1:0),limb.color.rgb.r,limb.color.rgb.g,limb.color.rgb.b,limb.thickness); s+=string(c);
  sprintf(c,"freq(%i,%i,%i) ",limb.chan,limb.band,(limb.negative?1:0)); s+=string(c);
  sprintf(c,"cum(%i,%lg)(%i) ",limb.cum?1:0,limb.crate,limb.creflect); s+=string(c);
  if (shape.brush.type==ctBitmap) s+="bitmap("+shape.brush.bitmap+") ";
  bool shapevisible=(shape.brush.type!=ctNone);
  sprintf(c,"shape(%i,%i,%i,%i) ",shapevisible?1:0,shape.brush.rgb.r,shape.brush.rgb.g,shape.brush.rgb.b); s+=string(c);
  s+="unibrushc("+shape.brush.tostring()+") ";
  s+="unilinec("+limb.color.tostring()+") ";
  return s;  
}

void TStyle::fromstring(const string s)
{ name="";
  const char *start=s.c_str(), *end=start; while (*end!='\r' && *end!='\n' && *end!=0) end++;
  string copys(start,end-start);
  const char *c=copys.c_str();
  const char *p=strchr(c,'='); if (p==0) return;
  name=string(c,p-c);
  const char *q;
  shortcut=0; p=strstr(c,"Ctrl+"); if (p!=0) shortcut=p[5];
  limb.color.type=ctRGB; limb.color.rgb=RGBCOLOR(0,0,0); limb.thickness=1;
  p=strstr(c,"line"); if (p!=0) {q=p+4; int vis,r,g,b; sscanf(q,"(%i,%i,%i,%i,%lg)",&vis,&r,&g,&b,&limb.thickness); limb.color.type=(vis==0?ctNone:ctRGB); limb.color.rgb=RGBCOLOR(r,g,b);}
  limb.chan=0; limb.band=0; limb.negative=false; limb.cum=false; limb.crate=1.0;
  p=strstr(c,"freq"); if (p!=0) {q=p+4; int neg; sscanf(q,"(%i,%i,%i)",&limb.chan,&limb.band,&neg); limb.negative=(neg!=0);}
  p=strstr(c,"cum"); if (p!=0)
  { q=p+3; int c,d; int ok=sscanf(q,"(%i,%lg)(%i)",&c,&limb.crate,&d);
    limb.cum=(c!=0); if (ok==3) limb.creflect=d;
  }
  p=strstr(c,"bitmap"); if (p!=0) {p=p+7; q=p; while (*q!=')' && *q!=0) q++; shape.brush.bitmap=string(p,q-p);}
  shape.brush.type=ctRGB; shape.brush.rgb=RGBCOLOR(0,0,0);
  p=strstr(c,"shape"); if (p!=0) {q=p+5; int has,r,g,b; sscanf(q,"(%i,%i,%i,%i)",&has,&r,&g,&b); shape.brush.rgb=RGBCOLOR(r,g,b); if (has==0) shape.brush.type=ctNone; else if (shape.brush.bitmap=="") shape.brush.type=ctRGB; else shape.brush.type=ctBitmap;}
  //
  p=strstr(c,"unibrushc");
  if (p!=0) {p=p+10; q=p; while (*q!=')' && *q!=0) q++; shape.brush.fromstring(string(p,q-p));}
  p=strstr(c,"unilinec");
  if (p!=0) {p=p+9; q=p; while (*q!=')' && *q!=0) q++; limb.color.fromstring(string(p,q-p));}
}


int TEffect::eindex(const double f,double *fstart,double *fend) const
{ double tot=0; unsigned int segment;
  for (segment=0; segment<cols.size()-1; segment++)
  { double thisf=fracs[segment];
    if (f<tot+thisf) break;
    tot+=fracs[segment];
  }
  double nexttot=tot+fracs[segment]; if (segment==cols.size()-1) nexttot=1;
  if (fstart!=0) *fstart=tot;
  if (fend!=0) *fend=nexttot;
  return segment;
}

string TEffect::tostring() const
{ string s(""); char c[1000];
  // name=freq(chan,band,negative,cumulative,cumrate,f,creflect) [rgb(frac,grad,r,g,b) | bmp(frac,bmp)]+
  sprintf(c,"%s=",name.c_str()); s+=string(c);
  sprintf(c,"freq(%i,%i,%i,%i,%lg,%lg)(%i) ",chan,band,negative?1:0,cumulative?1:0,cumrate,f,creflect); s+=string(c);
  for (int i=0; i<(int)cols.size(); i++)
  { if (cols[i].type==ctRGB) sprintf(c,"rgb(%lg,%i,%i,%i,%i) ",fracs[i],cols[i].fadetonext?1:0,cols[i].rgb.r,cols[i].rgb.g,cols[i].rgb.b);
    else if (cols[i].type==ctBitmap) {string gb=garble(cols[i].bitmap); sprintf(c,"bmp(%lg,%s) ",fracs[i],gb.c_str());}
    else if (cols[i].type==ctNone) sprintf(c,"none(%lg) ",fracs[i]);
    else LUASSERTMM("effect:tostring unknown color");
    s+=string(c);
  }
  return s;  
}

void TEffect::fromstring(const string s)
{ name=""; cols.clear(); fracs.clear();
  const char *start=s.c_str(), *end=start; while (*end!='\r' && *end!='\n' && *end!=0) end++;
  string copys(start,end-start); const char *c=copys.c_str(), *q;
  const char *p=strchr(c,'='); if (p==0) return;
  name=string(c,p-c);
  //
  chan=0; band=0; negative=false; cumulative=false; cumrate=1.0; f=0.1;
  p=strstr(c,"freq"); if (p!=0) {q=p+4; int cum,neg; sscanf(q,"(%i,%i,%i,%i,%lg,%lg)(%i)",&chan,&band,&neg,&cum,&cumrate,&f,&creflect); negative=(neg!=0); cumulative=(cum!=0);}
  //
  while (p!=0)
  { const char *crgb=strstr(p,"rgb"), *cbmp=strstr(p,"bmp"), *cnone=strstr(p,"none");
    p=0; if (crgb!=0) p=crgb;
    if (cbmp!=0) {if (p==0 || cbmp<p) p=cbmp;}
    if (cnone!=0) {if (p==0 || cnone<p) p=cnone;}
    if (p==0) break;
    if (p==crgb)
    { q=p+3; double f; int grad,r,g,b; sscanf(q,"(%lg,%i,%i,%i,%i)",&f,&grad,&r,&g,&b);
      fracs.push_back(f);
      TColor col; col.type=ctRGB; col.fadetonext=(grad!=0); col.rgb=RGBCOLOR(r,g,b); cols.push_back(col);
    }
    else if (p==cbmp)
    { q=p+3; double f; sscanf(q,"(%lg,",&f);
      const char *bmpstart=q; while (*bmpstart!=',' && *bmpstart!=0) bmpstart++; if (bmpstart!=0) bmpstart++;
      const char *bmpend=bmpstart; while (*bmpend!=0 && *bmpend!=')') bmpend++;
      if (bmpend!=0) {fracs.push_back(f); TColor col; col.type=ctBitmap; col.bitmap=ungarble(string(bmpstart,bmpend-bmpstart)); cols.push_back(col);}
    }
    else if (p==cnone)
    { q=p+4; double f; sscanf(q,"(%lg",&f);
      TColor col; col.type=ctNone; cols.push_back(col);
      fracs.push_back(f);
    }
    p++;
  }
}



string TColor::cttostring(ColorType ct)
{ if (ct==ctNone) return "ctNone";
  else if (ct==ctDefault) return "ctDefault";
  else if (ct==ctRGB) return "ctRGB";
  else if (ct==ctBitmap) return "ctBitmap";
  else if (ct==ctEffect) return "ctEffect";
  LUASSERTMM("unknown ct"); return "ctNone";
}

ColorType TColor::ctfromstring(const string s)
{ if (s=="ctNone") return ctNone;
  else if (s=="ctDefault") return ctDefault;
  else if (s=="ctRGB") return ctRGB;
  else if (s=="ctBitmap") return ctBitmap;
  else if (s=="ctEffect") return ctEffect;
  LUASSERTMM("unrecognised ct"); return ctDefault;
}



string TColor::tostring() const
{ // this will fit inside parentheses in the save data, so we don't use
  // parentheses and we garble our text
  string s, ss;
  s+="type("+cttostring(type)+","+cttostring(otype)+","+cttostring(dtype)+") ";
  s+="rgb("+StringInt(rgb.r)+","+StringInt(rgb.g)+","+StringInt(rgb.b)+") ";
  s+="fade() ";
  s+="cbmp("+garble(bitmap)+") ";
  s+="effect("+StringFloat(ef)+","+garble(effect)+") ";
  return s;
};

void TColor::fromstring(const string s)
{ const char *p,*q;
  type=ctNone; otype=ctRGB; dtype=ctNone; p=strstr(s.c_str(),"type("); if (p!=0)
  { p+=5; q=p; while (*q!=',' && *q!=')' && *q!=0) q++;
    type=ctfromstring(string(p,q-p));
    if (*q!=0) p=q+1; q=p; while (*q!=',' && *q!=')' && *q!=0) q++;
    otype=ctfromstring(string(p,q-p));
    if (*q!=0) p=q+1; q=p; while (*q!=')' && *q!=0) q++;
    dtype=ctfromstring(string(p,q-p));
  }
  rgb=RGBCOLOR(128,128,128); p=strstr(s.c_str(),"rgb("); if (p!=0)
  { p+=3; int r,g,b; sscanf(p,"(%i,%i,%i)",&r,&g,&b); rgb=RGBCOLOR(r,g,b);
  }
  fadetonext=false; p=strstr(s.c_str(),"fade("); if (p!=0) fadetonext=true;
  bitmap=""; p=strstr(s.c_str(),"cbmp("); if (p!=0)
  { p+=5; q=p; while (*q!=')' && *q!=0) q++;
    bitmap=ungarble(string(p,q-p));
  }
  ef=0.4; ereflect=0; effect=""; p=strstr(s.c_str(),"effect("); if (p!=0)
  { p+=6; sscanf(p,"(%lg,",&ef);
    while (*p!=',' && *p!=')' && *p!=0) p++; if (*p==',') p++; else p=0;
    q=p; while (q!=0 && *q!=')' && *q!=0) q++;
    if (p!=0) effect=ungarble(string(p,q-p));
  }
}



TBody::TBody()
{ limbs=0; nlimbs=0;
  version=STK_CORE_S; category=""; fps=30;
  NewFile();
}
TBody::~TBody()
{ if (limbs!=0) delete[] limbs; limbs=0; nlimbs=0;
  for (vector<TBmp>::iterator i=bmps.begin(); i!=bmps.end(); i++) i->release();
  bmps.clear();
}


void fix_color(TBody *body, TColor &col,bool foredit)
{ col.dtype=col.type; if (col.type!=ctEffect) return;
  const TEffect &effect = body->effects[col.eindex];
  double fstart,fend; int segment=effect.eindex(col.ef,&fstart,&fend);
  col.dtype=effect.cols[segment].type;
  if (col.dtype==ctNone) {}
  else if (col.dtype==ctRGB)
  { col.rgb=effect.cols[segment].rgb;
    if (col.fadetonext)
    { TColor nextcol = effect.cols[(segment+1)%effect.cols.size()];
      if (nextcol.type==ctRGB)
      { double r1=col.rgb.r, g1=col.rgb.g, b1=col.rgb.b;
        double r2=nextcol.rgb.r, g2=nextcol.rgb.g, b2=nextcol.rgb.b;
        double f=(col.ef-fstart)/(fend-fstart);
        double r=r1*(1-f)+r2*f, g=g1*(1-f)+g2*f, b=b1*(1-f)+b2*f;
        col.rgb=RGBCOLOR((int)r,(int)g,(int)b);
      }
    }
  }
  else if (col.dtype==ctBitmap)
  { col.bindex=effect.cols[segment].bindex;
    if (foredit) col.bitmap=effect.cols[segment].bitmap;
  }
  else LUASSERTMM("unknown color");
}

void TBody::RecalcEffects(bool foredit)  // foredit means we copy also string names, not just bindex
{ // for most colours, we copy type into dtype (the "draw" type.
  // but for ctEffect, we instantiate it.
  for (int i=0; i<nlimbs; i++) fix_color(this,limbs[i].color,foredit);
  for (vector<TShape>::iterator i=shapes.begin(); i!=shapes.end(); i++)
  { if (i->limbs) continue;
    fix_color(this,i->brush,foredit);
    fix_color(this,i->line,foredit);
  }
}


void assign_freq_f(TBody *body, TColor &col, double freq[3][6], double cmul)
{ // freq is an array[3][6] of frequencies.
  // If the colour is an effect, then we set it or increment it cumulatively as it wants
  if (col.type!=ctEffect) return;
  TEffect &effect = body->effects[col.eindex];
  int chan=effect.chan, band=effect.band;
  double f=0; if (chan==0 || chan==1) f=freq[chan][band];
  else if (chan==3) f=freq[2][band];
  else if (chan==2 && !effect.cumulative) f=(1.0+freq[0][band]-freq[1][band])*0.5;
  else if (chan==2 && effect.cumulative) f=(freq[0][band]-freq[1][band])*0.5;
  else LUASSERTMM("unknown effect chan");
  //  
  if (effect.cumulative)
  { int dir=1; if (effect.negative) dir*=-1; if (col.ereflect<0) dir*=-1;
    double cf=effect.cumrate*f*cmul; if (effect.cumrate<0) cf=-effect.cumrate*cmul;
    col.ef += dir*0.01*cf;
    if (col.ereflect==0)
    { while (col.ef<0) col.ef+=1; while (col.ef>1) col.ef-=1;
    }
    else
    { for (;;)
      { if (col.ef>1) {col.ef=2-col.ef; col.ereflect*=-1;}
        else if (col.ef<0) {col.ef=-col.ef; col.ereflect*=-1;}
        else break;
      }
    }
  }
  else
  { if (effect.negative) col.ef=1-f;
    else col.ef=f;
  }
}


void TBody::AssignFreq(double freq[3][6], double cmul)
{ for (int n=0; n<nlimbs; n++)
  { TLimb &limb=limbs[n]; int chan=limb.chan, band=limb.band;
    double f; if (chan==0 || chan==1) f=freq[chan][band];
    else if (chan==2 && limb.cum) f=(freq[1][band]-freq[0][band])*0.5;
    else if (chan==2 && !limb.cum) f=(1.0+freq[1][band]-freq[0][band])*0.5;
    else if (chan==3) f=freq[2][band];
    else if (chan==4) f=limb.f;
    else f=0;
    if (chan<=3)
    { if (limb.cum)
      { int dir=1; if (limb.negative) dir*=-1; if (limb.creflect<0) dir*=-1;
        double cf=limb.crate*f*cmul; if (limb.crate<0) cf=-limb.crate*cmul;
        limb.f += dir*0.01*cf;
        if (limb.creflect==0)
        { while (limb.f<0) limb.f+=1; while (limb.f>1) limb.f-=1;
        }
        else
        { for (;;)
          { if (limb.f>1) {limb.f=2-limb.f; limb.creflect*=-1;}
            else if (limb.f<0) {limb.f=-limb.f; limb.creflect*=-1;}
            else break;
          }
        }
      }
      else
      { if (limb.negative) limb.f=1-f;
        else limb.f=f;
      }
    }

  #ifdef _DEBUG
    LUASSERT(limb.f>=-100 && limb.f<=100);
  #endif
  }
  // assign frequencies into cumulative colours
  for (int n=0; n<nlimbs; n++) assign_freq_f(this,limbs[n].color,freq,cmul);
  for (vector<TShape>::iterator si=shapes.begin(); si!=shapes.end(); si++)
  { assign_freq_f(this,si->brush,freq,cmul);
    assign_freq_f(this,si->line,freq,cmul);
  }
}


void TBody::Recalc()
{ TLimb *limb=&limbs[0];
  limb->x=0; limb->y=0;
  RecalcLimb(0);
  //
  // anch[1..4] says where north,east,south,west may be anchored
  // i.e. if anch[east] is set, then the stick must be offset such that anch[east]
  // gets plotted at the right hand side.
  double anch[5]; bool ab[5];
  for (int i=1; i<=4; i++) ab[i]=false;
  anch[1]=5; anch[2]=-5; anch[3]=-5; anch[4]=5;
  for (int n=1; n<nlimbs; n++)
  { TLimb *limb=&limbs[n];
    if (limb->anchor==1) {ab[1]=true; if (limb->y<anch[1]) anch[1]=limb->y;}
    if (limb->anchor==2) {ab[2]=true; if (limb->x>anch[2]) anch[2]=limb->x;}
    if (limb->anchor==3) {ab[3]=true; if (limb->y>anch[3]) anch[3]=limb->y;}
    if (limb->anchor==4) {ab[4]=true; if (limb->x<anch[4]) anch[4]=limb->x;}
  }
  anchx=0; anchy=0;
  if (ab[1]) anchy=-anch[1]-5.0;
  if (ab[2]) anchx=-anch[2]+5.0;
  if (ab[3]) anchy=-anch[3]+5.0;
  if (ab[4]) anchx=-anch[4]-5.0;
}





void TBody::RecalcLimb(int n)
{ TLimb *limb=&limbs[n], *root=&limbs[limb->root];
  RecalcAngles(limb);
  limb->x0 = root->x; limb->y0=root->y;
  double len=limb->length;
  if (limb->type==2 || limb->type==3)
  { if (limb->chan==4 && limb->band==0) len=limb->length;
    else len=limb->lmin*limb->f+limb->length*(1-limb->f); // a spring
  }
  limb->x = root->x+len*cos(limb->ang);
  limb->y = root->y+len*sin(limb->ang);
  //
  for (int i=limb->childi; children[i]!=0; i++) RecalcLimb(children[i]);
}

void TBody::RecalcAngles(TLimb *limb)
{ TLimb *root=&limbs[limb->root];
  double ang=limb->aoff; if (limb->aisoff && limb->root!=0) ang+=root->ang;
  limb->ang0=ang;
  limb->ang1=ang+limb->ascale;
  if (limb->type==2 || limb->type==3) // a spring
  { limb->ang=ang+limb->aspring;
  }
  else // a line or an arc
  { if (limb->chan==4 && limb->band==0)
    { if (limb->aisoff) limb->ang=root->ang+limb->aoff+limb->f; else limb->ang=limb->aoff+limb->f;
    }
    else limb->ang=ang+limb->f*limb->ascale;
  }
#ifdef _DEBUG
  LUASSERT(limb->ang<1000 && limb->ang>-1000);
#endif
}

int TBody::CreateLimb(TLimb &n)
{ TLimb limb;
  //
  limb.type=n.type;
  limb.color=n.color;
  limb.negative=n.negative;
  limb.root=n.root;
  limb.aisoff=n.aisoff;
  limb.aoff=n.aoff;
  limb.ascale=n.ascale;
  limb.aspring=n.aspring;
  limb.length=n.length;
  limb.lmin=n.lmin;
  limb.chan=n.chan;
  limb.band=n.band;
  limb.cum=n.cum; limb.crate=n.crate; limb.creflect=n.creflect;
  limb.anchor=n.anchor;
  limb.f=n.f;
  limb.thickness=n.thickness;
  limb.linestyle=n.linestyle;
  limb.freqstyle=n.freqstyle;
  //
  TLimb *nl=new TLimb[nlimbs+1];
  if (limbs != 0) for (int i = 0; i < nlimbs; i++) nl[i] = limbs[i];
  nl[nlimbs] = limb;
  if (limbs != 0) {delete[] limbs; limbs = nl; nlimbs++; }
  MakeChildren();
  return nlimbs-1;
}

void MakeShapesAvoidLimb(vector<TShape> *shapes,int n,int root)
{ // this routine will make sure that all the shapes avoid n.
  // they will try to point to root instead. If that doesn't work,
  // the shape will be deleted. 
  for (vector<TShape>::iterator s=shapes->begin(); s!=shapes->end();)
  { TShape &shape = *s;
    if (shape.limbs) {s++; continue;}
    list<TJointRef> nss;
    for (int i=0; i<(int)shape.p.size(); i++)
    { TJointRef old=shape.p[i];
      if (old.i!=n) nss.push_back(old);
      else if (root!=n) nss.push_back(TJointRef(root));
    }
    nss.unique(); // to remove adjacent duplicates
    shape.p.clear(); shape.p.reserve(nss.size()+1);
    for (list<TJointRef>::const_iterator i=nss.begin(); i!=nss.end(); i++) shape.p.push_back(*i);
    // but if the shape now has only a single point, then it's done for
    TJointRef pt=shape.p[0]; bool issingle=true;
    for (int i=1; i<(int)shape.p.size() && issingle; i++) {if (shape.p[i]!=pt) issingle=false;}
    if (issingle) s=shapes->erase(s);
    else s++;
  }
}

int TBody::DeleteLimb(int n)
{ // NOTE: the editor routine 'deletelimb' requires that this Body::DeleteLimb
  // preserves the order of limbs: i.e. that it doesn't pop the last one into
  // the middle of the list.
  if (n==0) return n;
  int root=limbs[n].root;
  MakeShapesAvoidLimb(&shapes,n,root);
  // now remove it from the limbs list, and fixup references to it
  for (int s=0; s<(int)shapes.size(); s++)
  { TShape &shape = shapes[s];
    for (int i=0; i<(int)shape.p.size(); i++)
    { if (shape.p[i].i>=n) shape.p[i].i=shape.p[i].i-1;
    }
  }
  nlimbs--;
  vector<TLimb> newl; newl.resize(nlimbs);
  for (int i=0; i<=nlimbs; i++)
  { int src=i; int dst=i; if (dst==n) dst=-1; else if (dst>n) dst=i-1;
    if (dst!=-1)
    { newl[dst] = limbs[src];
      int r=limbs[src].root; if (r==n) r=root; else if (r>n) r=r-1; newl[dst].root=r;
      if (r==0) newl[dst].aisoff=false;
    }
  }
  //
  if (limbs!=0) delete[] limbs; limbs=new TLimb[newl.size()];
  for (size_t i = 0; i<newl.size(); i++) limbs[i] = newl[i];
  nlimbs=(int)newl.size();
  MakeChildren();
  //
  if (root>n) root--; int sel=root;
  // now find the limb that came after it
  for (int i=nlimbs-1; i>0; i--) if (limbs[i].root==root) sel=i;
  return sel;
}

void RecMarkLimbUnneeded(int nlimbs,TLimb *limbs,vector<int> &children, bool *keep,int n)
{ keep[n]=false;
  for (int i=limbs[n].childi; children[i]!=0; i++) RecMarkLimbUnneeded(nlimbs,limbs,children,keep,children[i]);
}

int TBody::DeleteBranch(int n,vector<int> *remap)
{ int oldroot=limbs[n].root;
  bool *keep=new bool[nlimbs];
  for (int i=0; i<nlimbs; i++) keep[i]=true;
  RecMarkLimbUnneeded(nlimbs,limbs,children,keep,n);
  for (int i=0; i<nlimbs; i++)
  { if (!keep[i]) MakeShapesAvoidLimb(&shapes,i,oldroot); // because oldroot won't be deleted!
  }
  int *newi=new int[nlimbs]; int tot=0; // newi[oldpos]=newpos
  for (int i=0; i<nlimbs; i++)
  { newi[i]=tot; if (keep[i]) tot++;
  }
  for (int i=0; i<nlimbs && remap!=NULL; i++)
  { if (keep[i]) (*remap)[i]=newi[i]; else (*remap)[i]=-1;
  }
  // fixup any shape references
  for (int s=0; s<(int)shapes.size(); s++)
  { TShape &shape = shapes[s];
    for (int i=0; i<(int)shape.p.size(); i++)
    { shape.p[i].i = newi[shape.p[i].i];
    }
  }
  //
  vector<TLimb> newl; newl.resize(tot); tot=0;
  for (int i=0; i<nlimbs; i++)
  { if (keep[i])
    { newl[tot] = limbs[i];
      int root=limbs[i].root; root=newi[root];
      newl[tot].root=root;
      tot++;
    }
  }
  int newroot=newi[oldroot];
  delete[] keep;
  delete[] newi;
  //
  if (limbs!=0) delete[] limbs; limbs=new TLimb[newl.size()];
  for (size_t i = 0; i < newl.size(); i++) limbs[i] = newl[i];
  nlimbs=(int)newl.size();
  MakeChildren();
  //
  return newroot;
}


int TBody::bindex(const string bmpname)
{ for (unsigned int i=0; i<bmps.size(); i++)
  { if (lustricmp(bmpname.c_str(), bmps[i].name.c_str())==0) return i;
  }
  return -1;
}

int TBody::eindex(const string ename)
{ for (unsigned int i=0; i<effects.size(); i++)
  { if (lustricmp(ename.c_str(), effects[i].name.c_str())==0) return i;
  }
  return -1;
}



void WriteLine(char *buf,unsigned int bufsize,int &len,const char *c)
{ unsigned int clen=(unsigned int)strlen(c);
  if (buf!=NULL && len+clen<bufsize) strcpy(buf+len,c);
  len=len+clen;
}

unsigned int TBody::WriteData(char *buf,unsigned int bufsize,int root)
{ list<int> roots; roots.push_back(root);
  return WriteData(buf,bufsize,roots);
}

string garble(const string s)
{ string g(""); char c[3]; c[1]='_'; c[2]=0;
  for (unsigned int i=0; i<s.length(); i++)
  { c[0]=s[i];
    g+=c;
  }
  return g;
}
string ungarble(const string s)
{ size_t len = s.length()/2;
  string u(len,0);
  for (size_t i=0; i<len; i++) u[i]=s[i*2];
  return u;
}

unsigned int TBody::WriteData(char *buf,unsigned int bufsize,list<int> &roots)
{ 
  char c[10240], d[10240];
  int len=0;
  // we'll just keep a bitmap of the roots
  vector<bool> rbitmap; rbitmap.resize(nlimbs);
  for (list<int>::const_iterator i=roots.begin(); i!=roots.end(); i++) rbitmap[*i]=true;
  //
  vector<bool> keep; keep.resize(nlimbs,false);
  for (list<int>::const_iterator i=roots.begin(); i!=roots.end(); i++) keep[*i]=true;
  bool changed=true; 
  while (changed)
  { changed=false;
    for (int n=0; n<nlimbs; n++)
    { int r=limbs[n].root;
      if (keep[r] && !keep[n]) {keep[n]=true; changed=true;}
    }
  }
  vector<int> newi; newi.resize(nlimbs); int tot=0; // meaning: newi[oldloc] is the newloc
  for (int n=0; n<nlimbs; n++)
  { if (keep[n]) {newi[n]=tot; tot++;}
  }
  //
  // Note: we will only keep those shapes for whom all of their points are being kept
  int nkeepshapes=0; for (int s=0; s<(int)shapes.size(); s++)
  { TShape &shape = shapes[s]; bool ok=true;
    for (int i=0; i<(int)shape.p.size(); i++)
    { TJointRef pt = shape.p[i];
      if (!keep[pt.i]) ok=false;
    }
    if (ok) nkeepshapes++;
  }
  //
  // HEADER
  //
  sprintf(c,"nlimbs=%i\r\n",tot); WriteLine(buf,bufsize,len,c);
  sprintf(c,"nshapes=%i\r\n",nkeepshapes); WriteLine(buf,bufsize,len,c);
  string rs=""; for (list<int>::const_iterator i=roots.begin(); i!=roots.end(); i++)
  { if (rs!="") rs+=","; rs+=StringInt(newi[*i]);
  }
  rs="root="+rs+"\r\n"; WriteLine(buf,bufsize,len,rs.c_str());
  sprintf(c,"version=%s\r\n",version.c_str()); WriteLine(buf,bufsize,len,c);
  sprintf(c,"category=%s\r\n",category.c_str()); WriteLine(buf,bufsize,len,c);
  sprintf(c,"copyright=%s\r\n",copyright.c_str()); WriteLine(buf,bufsize,len,c);
  sprintf(c,"neffects=%i\r\n",effects.size()); WriteLine(buf,bufsize,len,c);
  sprintf(c,"fps=%i\r\n",fps); WriteLine(buf,bufsize,len,c);
  //
  // LIMBS
  //
  for (int n=0; n<nlimbs; n++)
  { if (keep[n])
    { TLimb &limb=limbs[n];
      sprintf(c,"limb(%i)= ",newi[n]);
      if (limb.type==0) strcat(c,"line() "); else if (limb.type==1) strcat(c,"arc() ");
      else if (limb.type==2) strcat(c,"spring() "); else if (limb.type==3) strcat(c,"circle() ");
      else LUASSERTMM("limb type");
      if (limb.color.type==ctNone) strcat(c,"invisible() ");
      if (limb.negative) strcat(c,"negative() ");
      //
      int r=limb.root; if (rbitmap[n]) r=n; sprintf(d,"root(%i) ",newi[r]); strcat(c,d);
      if (limb.aisoff) {sprintf(d,"ang(%.9g,%.9g)(%.9g) ",limb.aoff,limb.ascale,limb.aspring); strcat(c,d);}
      else {sprintf(d,"angabs(%.9g,%.9g)(%.9g) ",limb.aoff,limb.ascale,limb.aspring); strcat(c,d);}
      //
      sprintf(d,"lmin(%.9g) ",limb.lmin); strcat(c,d);
      sprintf(d,"length(%.9g) ",limb.length); strcat(c,d);
      sprintf(d,"freq(%i,%i) ",limb.chan,limb.band); strcat(c,d);
      sprintf(d,"cum(%i,%g)(%i) ",limb.cum?1:0,limb.crate,limb.creflect); strcat(c,d);
      if (limb.anchor!=0) {sprintf(d,"anchor(%i) ",limb.anchor); strcat(c,d);}
      sprintf(d,"frac(%.9g) ",limb.f); strcat(c,d);
      sprintf(d,"col(%i,%i,%i) ",limb.color.rgb.r,limb.color.rgb.g,limb.color.rgb.b); strcat(c,d);
      sprintf(d,"thickness(%.9g) ",limb.thickness); strcat(c,d);
      if (limb.linestyle!="")
      { string ss=garble(limb.linestyle);
        sprintf(d,"linestyle(%s) ",ss.c_str()); strcat(c,d);
      }
      if (limb.freqstyle!="") 
      { string ss=garble(limb.freqstyle);
        sprintf(d,"freqstyle(%s) ",ss.c_str()); strcat(c,d);
      }
      string ss=limb.color.tostring();
      sprintf(d,"unicol(%s) ",ss.c_str()); strcat(c,d);

      strcat(c,"\r\n");
      WriteLine(buf,bufsize,len,c);
    }
  }
  //
  // SHAPES
  // remember: only keep shapes wholly contained from the root
  for (int nks=0, s=0; s<(int)shapes.size(); s++)
  { const TShape &shape=shapes[s]; bool okay=true;
    for (int i=0; i<(int)shape.p.size(); i++)
    { TJointRef pt = shape.p[i];
      if (!keep[pt.i]) okay=false;
    }
    if (!okay) continue;
    sprintf(c,"shape(%i)= ",nks);
    if (shape.limbs) strcat(c,"limbs()");
    else
    { if (shape.brush.dtype==ctNone) {}
      else if (shape.brush.dtype==ctBitmap)
      { string ss=garble(shape.brush.bitmap); sprintf(d,"bitmap(%s) brush(128,128,128) ",ss.c_str());
        strcat(c,d);
      }
      else if (shape.brush.dtype==ctRGB) {sprintf(d,"brush(%i,%i,%i) ",shape.brush.rgb.r,shape.brush.rgb.g,shape.brush.rgb.b); strcat(c,d);}
      else if (shape.brush.dtype==ctDefault) strcat(c,"brush(-1,-1,-1) ");
      else LUASSERTMM("unexpected shape color");
      //
      if (shape.balternate) strcat(c,"alternate() ");
      if (shape.line.dtype==ctNone) {}
      else if (shape.line.dtype==ctRGB) {sprintf(d,"line(%.9g,%i,%i,%i) ",shape.thickness,shape.line.rgb.r,shape.line.rgb.g,shape.line.rgb.b); strcat(c,d);}
      else LUASSERTMM("unexpected shape line");
      //
      if (shape.linestyle!="")
      { string ss=garble(shape.linestyle);
        sprintf(d,"linestyle(%s) ",ss.c_str()); strcat(c,d);
      }
      if (shape.fillstyle!="")
      { string ss=garble(shape.fillstyle);
        sprintf(d,"fillstyle(%s) ",ss.c_str()); strcat(c,d);
      }
      strcat(c,"points("); string s("");
      for (int j=0; j<(int)shape.p.size(); j++)
      { TJointRef pt = shape.p[j];
        if (j!=0) strcat(c,","); sprintf(d,"%i",newi[pt.i]); strcat(c,d);
        if (pt.ar) {if (s!="") s+=","; sprintf(d,"%i",j); s+=d;}
      }
      strcat(c,") ");
      if (s!="") {s="arcroots("+s+") "; strcat(c,s.c_str());}
      //
      s=shape.brush.tostring(); sprintf(d,"unibrush(%s) ",s.c_str()); strcat(c,d);
      s=shape.line.tostring(); sprintf(d,"unilinec(%s) ",s.c_str()); strcat(c,d);
    }
    strcat(c,"\r\n");
    WriteLine(buf,bufsize,len,c);
    nks++;
  }
  //
  // EFFECTS
  //
  for (int ne=0; ne<(int)effects.size(); ne++)
  { const TEffect &effect=effects[ne];
    string es=effect.tostring();
    sprintf(c,"effect(%i)= %s\r\n",ne,es.c_str());
    WriteLine(buf,bufsize,len,c);
  }


  //
  return len+1;  
}





bool GetEntry(const char **lines,int nlines,const char *key,const char **buf,const char **end,int &pos)
{ for (int i=0; i<nlines; i++)
  { const char *c=lines[pos]; int len=(int)strlen(key);
    if (strncmp(c,key,len)==0)
    { c=c+len; while (*c==' ') c++;
      if (*c=='=')
      { c++; while (*c==' ') c++;
        *buf=c; pos++; *end=lines[pos];
        return true;
      }
    }
    pos++; if (pos>=nlines) pos=0;
  }
  return false;
}


// lustr - like strstr, but it respects () hierarchy, and will only
// match strings at the start of a word
inline bool luisalpha(const char c) {return ((c>='a'&&c<='z') || (c>='A' && c<='Z') || (c=='_') || (c>='0' && c<='9'));}
//
char *lustr(char *src,const char *find)
{ char *c=src; size_t len=strlen(find);
  while (*c!=0)
  { if (strncmp(c,find,len)==0) return c;
    else if (*c=='(') {while (*c!=')' && *c!=0) c++; if (*c==')') c++;}
    else if (luisalpha(*c)) {c++; while (luisalpha(*c)) c++;}
    else c++;
  }
  return 0;
}

// closing - if c points to "fred(stuff() stuff())" then we return a pointer to the terminal parenthesis
char *closing(char *c)
{ int depth=0;
  while (*c!=0)
  { if (*c=='(') depth++;
    else if (*c==')') {depth--; if (depth==0) return c;}
    c++;
  }
  return 0;
}



bool TBody::ReadData(const char *buf,char *err,ReadDataFlags flags,list<int> *newroots) // returns index of new root nodes
{ bool overwrite = (flags&rdOverwrite)!=0;
  bool strict = (flags&rdStrict)!=0;
  //
  if (newroots!=NULL) newroots->clear();
  const char *bpos=buf; unsigned int flines=0; const char *lines[10240]; //bool ok=true;
  while (*bpos!=0)
  { const char *newline=bpos;
    while (*newline!='\r' && *newline!='\n' && *newline!=0) newline++;
    if (*newline!=0)
    { //*newline=0; don't do this, since it stops it being readonly!
      newline++; while (*newline=='\r' || *newline=='\n') newline++;
      if (flines<10240) lines[flines]=bpos;
      flines++;
    }
    bpos=newline;
  }
  lines[flines]=bpos; flines++; // past the final one
  int nlines=flines; if (nlines>10240) nlines=10240;
  int pos=0; char d[10240]; const char *oentry,*end; char entry[1024];
  //
  bool bres=GetEntry(lines,nlines,"nlimbs",&oentry,&end,pos); if (!bres) {sprintf(err,"File is not a stick file. Has no nlimbs="); return false;}
  strncpy(entry,oentry,end-oentry); entry[end-oentry]=0;
  int nl; int ires=sscanf(entry,"%i",&nl); if (ires==0 || ires==EOF || nl<0 || nl>10240) { sprintf(err,"File has an invalid nlimbs="); return false;}
  int ns=0; bres=GetEntry(lines,nlines,"nshapes",&oentry,&end,pos);
  if (bres) {strncpy(entry,oentry,end-oentry); entry[end-oentry]=0; sscanf(entry,"%i",&ns);}
  int ne=0; bres=GetEntry(lines,nlines,"neffects",&oentry,&end,pos);
  if (bres) {strncpy(entry,oentry,end-oentry); entry[end-oentry]=0; sscanf(entry,"%i",&ne);}
  list<int> roots; bres=GetEntry(lines,nlines,"root",&oentry,&end,pos); if (bres)
  { strncpy(entry,oentry,end-oentry); entry[end-oentry]=0; // roots=1,2,3,4,5
    char *c=entry; while (*c!=0)
    { int r, ires=sscanf(c,"%i",&r);
      if (ires!=1) {sprintf(err,"shape has unrecognised roots:\r\n%s",entry); return false;}
      roots.push_back(r);
      while (*c!=',' && *c!=0) c++; if (*c==',') c++;
    }
  }
  double sver=3.4; bres=GetEntry(lines,nlines,"version",&oentry,&end,pos);
  if (bres) {strncpy(entry,oentry,end-oentry); entry[end-oentry]=0; sscanf(entry,"%lg",&sver);}
  double cver=STK_CORE;
  version=STK_CORE_S;
  toonewversion = (sver>cver);
  category=""; bres=GetEntry(lines,nlines,"category",&oentry,&end,pos);
  if (bres) {category=StringTrim(string(oentry,end-oentry));}
  copyright=""; bres=GetEntry(lines,nlines,"copyright",&oentry,&end,pos);
  if (bres) {copyright=StringTrim(string(oentry,end-oentry));}
  if (overwrite)
  { bres=GetEntry(lines,nlines,"fps",&oentry,&end,pos);
    if (bres) sscanf(oentry,"%i",&fps);
  }
  //

  //
  // LIMBS
  //
  vector<TLimb> nls; nls.resize(nl);
  // we reset element zero here, because old files didn't save its value.
  if (overwrite)
  { nls[0].aoff=0; nls[0].ascale=2*pi;
    nls[0].type=0; nls[0].color.type=ctRGB; nls[0].color.rgb=RGBCOLOR(0,0,0);
    nls[0].root=0; nls[0].aisoff=false; nls[0].chan=4; nls[0].band=0; nls[0].anchor=0; nls[0].ang=0;
    nls[0].f=0;
  }
  //
  for (int n=0; n<nl; n++)
  { TLimb &limb=nls[n];
    sprintf(d,"limb(%i)",n);
    bres=GetEntry(lines,nlines,d,&oentry,&end,pos);
    if (!bres) {sprintf(err,"Unable to find limb(%i)",n); return false;}
    strncpy(entry,oentry,end-oentry); entry[end-oentry]=0;
    // now we have c and must decode it.
    // "line/arc/spring [invisible] root(r) ang/angabs(off,scale) [lmin(l)] length(l) freq(b,c) frac(f)"
    limb.type=0; char *p=lustr(entry,"arc"); if (p!=NULL) limb.type=1;
    p=lustr(entry,"spring"); if (p!=NULL) limb.type=2;
    p=lustr(entry,"circle"); if (p!=NULL) limb.type=3;
    limb.color.type=ctRGB; limb.color.otype=ctRGB; p=lustr(entry,"invisible"); if (p!=NULL) limb.color.type=ctNone;
    limb.color.dtype=limb.color.type; limb.color.bitmap=""; limb.color.effect="";
    limb.negative=false; p=lustr(entry,"negative"); if (p!=NULL) limb.negative=true;
    p=lustr(entry,"root("); char *q=p+4; if (p==NULL) {sprintf(err,"limb has no root:\r\n%s",entry); return false;}
    ires=sscanf(q,"(%i)",&limb.root); if (ires<1) {sprintf(err,"limb has invalid root:\r\n%s",entry); return false;}
    limb.aisoff=false; p=lustr(entry,"angabs("); if (p!=NULL) q=p+6;
    if (p==NULL) {limb.aisoff=true; p=lustr(entry,"ang("); if (p!=NULL) q=p+3;}
    if (p==NULL) {sprintf(err,"limb has no angle:\r\n%s",entry); return false;}
    // old files wrote (aoff,ascale) and left aspring=0
    // new files write (aoff,ascale)(aspring)
    ires=sscanf(q,"(%lg,%lg)(%lg)",&limb.aoff,&limb.ascale,&limb.aspring);
    if (ires<3)
    { ires=sscanf(q,"(%lg,%lg)",&limb.aoff,&limb.ascale); if (ires<2) {sprintf(err,"limb has invalid angles:\r\n%s",entry); return false;}
      limb.aspring=0;
    }
    p=lustr(entry,"length("); if (p==NULL) {sprintf(err,"limb has no length:\r\n%s",entry); return false;}
    q=p+6; ires=sscanf(q,"(%lg)",&limb.length); if (ires<1) {sprintf(err,"limb has invalid length:\r\n%s",entry); return false;}
    p=lustr(entry,"lmin("); if (p==NULL) {limb.lmin=limb.length*0.9;}
    else {q=p+4; ires=sscanf(q,"(%lg)",&limb.lmin); if (ires<1) {sprintf(err,"limb has invalid lmin:\r\n%s",entry); return false;}}
    p=lustr(entry,"freq("); if (p==NULL) {sprintf(err,"limb has no freq:\r\n%s",entry); return false;}
    q=p+4; ires=sscanf(q,"(%i,%i)",&limb.chan,&limb.band); if (ires<2) {sprintf(err,"limb has invalid freq:\r\n%s",entry); return false;}
    // old files sometimes mistakenly had chan=3. I don't know why.
    if (sver<3.7)
    { if (limb.chan==3) {limb.chan=4; limb.band=0;}
    }
    limb.cum=false; limb.crate=1; limb.creflect=0; p=lustr(entry,"cum(");
    if (p!=0) {q=p+3; int ic=0; ires=sscanf(q,"(%i,%lg)(%i)",&ic,&limb.crate,&limb.creflect); limb.cum=(ic!=0); if (ires<2) {sprintf(err,"limb has invalid cumulative:\r\n%s",entry); return false;}}
    p=lustr(entry,"col("); int r=-1,g=-1,b=-1; if (p==NULL) {limb.color.rgb=RGBCOLOR(255,255,255);}
    else {q=p+3; ires=sscanf(q,"(%i,%i,%i)",&r,&g,&b); if (ires<3) {sprintf(err,"limb has invalid color:\r\n%s",entry); return false;}}
    limb.color.rgb=RGBCOLOR(r,g,b); if ((r==-1 || g==-1 || b==-1) && limb.color.type==ctRGB) {limb.color.type=ctDefault; limb.color.dtype=ctDefault;}
    p=lustr(entry,"thickness("); q=p+9; if (p==NULL) {limb.thickness=1.0;}
    else {ires=sscanf(q,"(%lg)",&limb.thickness); if (ires<1) {sprintf(err,"limb has invalid thickness\r\n%s",entry); return false;}}
    limb.anchor=0; p=lustr(entry,"anchor(");
    if (p!=NULL)
    { q=p+6; ires=sscanf(q,"(%i)",&limb.anchor);
      if (ires<1) {sprintf(err,"limb has invalid anchor:\r\n%s",entry); return false;}
    }
    limb.f=(double)(rand()%100)/200.0 + 0.25; p=lustr(entry,"frac(");
    if (p!=NULL) {q=p+4; sscanf(q,"(%lg)",&limb.f);}
    if (sver<3.46) // earlier versions had a screwed-up thing with fixed lines
    { if ((limb.type==0 || limb.type==1) && limb.chan==4 && limb.band==1)
      { limb.band=0; limb.f*=limb.ascale;
      }
      else if ((limb.type==2 || limb.type==3) && limb.chan==4 && limb.band==1)
      { limb.band=0; limb.f*=limb.ascale;
      }
      else if ((limb.type==0 || limb.type==1) && limb.chan==4 && limb.band==0)
      { limb.f -= limb.aoff;
      }
    }
    if (strict)
    { if (limb.f<-100 || limb.f>100) {LUASSERTMM("out-of-range f"); limb.f=0.2;}
      if (limb.type==2 || limb.type==3)
      { if (limb.aspring<-100 || limb.aspring>100) {LUASSERTMM("out-of-range aspring"); limb.aspring=0.3;}
      }
    }
    limb.linestyle=""; p=lustr(entry,"linestyle(");
    if (p!=NULL) {p=p+10; q=p; while (*q!=')' && *q!=0) q++; limb.linestyle=ungarble(string(p,q-p));}
    limb.freqstyle=""; p=lustr(entry,"freqstyle(");
    if (p!=NULL) {p=p+10; q=p; while (*q!=')' && *q!=0) q++; limb.freqstyle=ungarble(string(p,q-p));}
    // we have set color above, as per older versions. Now we overwrite with newer data, if it's there.
    p=lustr(entry,"unicol(");
    if (p!=NULL) {q=closing(p); p+=7; limb.color.fromstring(string(p,q-p));}
  }
  if (sver<3.44)
  { nls[0].color.type=ctRGB; nls[0].color.rgb=RGBCOLOR(0,0,0);
    // background colour wasn't stored in the file back then...
  }
  // some other resetting of element 0:
  nls[0].thickness=0; 
  //if (nls[0].chan!=4 || nls[0].chan!=1) nls[0].f=0;
  if (nls[0].length==0) nls[0].f=0;

  //
  // SHAPES
  //
  vector<TShape> nss; nss.reserve(ns+2);
  for (int n=0; n<ns; n++)
  { sprintf(d,"shape(%i)",n); bres=GetEntry(lines,nlines,d,&oentry,&end,pos);
    if (!bres) {sprintf(err,"Unable to find shape(%i)",n); return false;}
    strncpy(entry,oentry,end-oentry); entry[end-oentry]=0;
    // now we have entry and must decode it.
    // "limbs  |||  [bitmap(bmp)] [brush(r,g,b)] [line(thick,r,g,b)] points(i0,i1,...) [arcroots(1,3)] [alternate]"
    TShape shape; shape.limbs=false; shape.brush.type=ctNone; shape.brush.bindex=-1; shape.line.type=ctNone; char *p;
    if (lustr(entry,"limbs")!=NULL) shape.limbs=true;
    else
    { shape.brush.type=ctNone; 
      int r=-1, g=-1, b=-1; bool bvisible=false, balternate=false; ; char *q;
      string bitmap; p=lustr(entry,"bitmap(");
      if (p!=NULL) {p=p+7; q=p; while (*q!=')' && *q!=0) q++; bitmap=ungarble(string(p,q-p));}
      p=lustr(entry,"brush(");
      if (p!=NULL) {q=p+5; bvisible=true; ires=sscanf(q,"(%i,%i,%i)",&r,&g,&b); if (ires!=3) {sprintf(err,"shape has invalid brush:\r\n%s",entry); return false;}}
      p=lustr(entry,"alternate"); if (p!=NULL) balternate=true;
      else {if (sver<3.48) balternate=true; else balternate=false;}
      // previously, silence meant "alternate-fill-mode". Now, silence means "winding"
      if (bitmap!="" && sver<3.625) bvisible=true;
      // older versions assumed that if a bitmap was written, then it was visible.
      // new versions don't, so as to keep more information around in the edit/undo buffer
      shape.brush.rgb=RGBCOLOR(r,g,b); shape.brush.bitmap=bitmap; shape.balternate=balternate;
      if (!bvisible) shape.brush.type=ctNone; else if (bitmap!="") shape.brush.type=ctBitmap; else shape.brush.type=ctRGB;
      shape.brush.dtype=shape.brush.type; shape.brush.otype=ctRGB;
      p=lustr(entry,"unibrush(");
      if (p!=0) {q=closing(p); p+=9; shape.brush.fromstring(string(p,q-p));}
      //
      r=-1; g=-1; b=-1; bool lvisible=false;
      p=lustr(entry,"line("); 
      if (p!=NULL) {q=p+4; lvisible=true; ires=sscanf(q,"(%lg,%i,%i,%i)",&shape.thickness,&r,&g,&b); if (ires!=4) {sprintf(err,"shape has invalid line:\r\n%s",entry); return false;}}
      shape.line.rgb=RGBCOLOR(r,g,b);
      if (!lvisible) shape.line.type=ctNone; else shape.line.type=ctRGB;
      shape.line.dtype=shape.line.type; shape.line.otype=ctRGB;
      p=lustr(entry,"unilinec(");
      if (p!=0) {q=closing(p); p+=9; shape.line.fromstring(string(p,q-p));}
      //
      p=lustr(entry,"points("); if (p==NULL) {sprintf(err,"shape has no points:\r\n%s",entry); return false;}
      q=p+6; int npts=1; while (*q!=')' && *q!=0) {if (*q==',') npts++; q++;} shape.p.reserve(npts+2);
      q=p+7; while (*q!=')' && *q!=0)
      { int p; ires=sscanf(q,"%i",&p);
        if (ires!=1) {sprintf(err,"shape has unrecognised points:\r\n%s",entry); return false;}
        shape.p.push_back(TJointRef(p));
        while (*q!=',' && *q!=0 && *q!=')') q++; if (*q==',') q++;
      }
      p=lustr(entry,"arcroots("); if (p==NULL) q=""; else q=p+9; while (*q!=')' && *q!=0)
      { int j; ires=sscanf(q,"%i,",&j);
        if (ires!=1) {sprintf(err,"shape has unrecognised arcroots:\r\n%s",entry); return false;}
        shape.p[j].ar=true;
        while (*q!=',' && *q!=0 && *q!=')') q++; if (*q==',') q++;
      }
      shape.linestyle=""; p=lustr(entry,"linestyle(");
      if (p!=NULL) {p=p+10; q=p; while (*q!=')' && *q!=0) q++; shape.linestyle=ungarble(string(p,q-p));}
      shape.fillstyle=""; p=lustr(entry,"fillstyle(");
      if (p!=NULL) {p=p+10; q=p; while (*q!=')' && *q!=0) q++; shape.fillstyle=ungarble(string(p,q-p));}
      if (strict)
      { if (shape.brush.type==ctBitmap && nls[shape.p[0].i].type!=3) {LUASSERTMM("bitmap in unbitmappable shape"); nls[shape.p[0].i].type=3;}
      }
    }
    nss.push_back(shape);
  }
  if (ns==0)
  { TShape s; s.limbs=true; nss.push_back(s);
  }

  //
  // EFFECTS
  //
  vector<TEffect> nes; nes.reserve(ne+2);
  for (int n=0; n<ne; n++)
  { sprintf(d,"effect(%i)",n); bres=GetEntry(lines,nlines,d,&oentry,&end,pos);
    if (!bres) {sprintf(err,"Unable to find effect(%i)",n); return false;}
    strncpy(entry,oentry,end-oentry); entry[end-oentry]=0;
    // now we have entry and must decode it. That's easy, using fromstring
    TEffect e; e.fromstring(entry);
    nes.push_back(e);
  }

      
  //
  // LIMBS
  //
  int off;
  if (overwrite)
  { if (limbs!=0) delete[] limbs; limbs=new TLimb[nl];
    off=0; nlimbs=nl;
  }
  else
  { TLimb *newlimbs=new TLimb[nlimbs+nl];
    if (limbs != 0) for (int i = 0; i < nlimbs; i++) newlimbs[i] = limbs[i];
    if (limbs!=0) delete[] limbs; limbs=newlimbs;
    off=nlimbs; nlimbs+=nl;
  }
  for (int n=off; n<nlimbs; n++)
  { limbs[n] = nls[n-off];
    limbs[n].root += off;
  }
  //
  // SHAPES
  //
  if (overwrite)
  { shapes.clear();
    shapes.assign(nss.begin(),nss.end());
  }
  else
  { vector<TShape>::iterator li = nss.end();
    for (vector<TShape>::iterator i=nss.begin(); i!=nss.end(); i++)
    { if (i->limbs) li=i;
      for (vector<TJointRef>::iterator j=i->p.begin(); j!=i->p.end(); j++)
      { j->i = j->i + off;
      }
    }
    if (li!=nss.end()) nss.erase(li);
    shapes.insert(shapes.end(),nss.begin(),nss.end());
  }
  //
  // EFFECTS
  //
  if (overwrite) {effects.clear(); effects.assign(nes.begin(),nes.end());}
  else
  { for (vector<TEffect>::const_iterator s=nes.begin(); s!=nes.end(); s++)
    { bool wasin=false;
      for (vector<TEffect>::iterator d=effects.begin(); d!=effects.end(); d++)
      { if (lustricmp(d->name.c_str(), s->name.c_str())==0) {*d=*s; wasin=true; break;}
      }
      if (!wasin) effects.push_back(*s);
    }
  }

  // now we cache the child-lists...
  MakeChildren();
  for (list<int>::const_iterator i=roots.begin(); newroots!=NULL && i!=roots.end(); i++)
  { newroots->push_back(off+*i);
  }
  if (roots.size()==0) {sprintf(err,"stick has no roots"); return false;}
  *err=0; return true;
}





void TBody::NewFile()
{ if (limbs!=0) delete[] limbs; nlimbs=0;
  shapes.clear();
  for (vector<TBmp>::iterator i=bmps.begin(); i!=bmps.end(); i++) i->release();
  bmps.clear();
  effects.clear();
  styles.clear();
  children.clear();
  category=""; fps=30;
  copyright="";
  version=STK_CORE_S; toonewversion=false;
  //
  limbs=new TLimb[1]; nlimbs=1;
  limbs[0].type=0; limbs[0].color.type=ctRGB; limbs[0].thickness=0; limbs[0].color.rgb=RGBCOLOR(0,0,0);
  limbs[0].linestyle=""; limbs[0].freqstyle="";
  limbs[0].root=0; limbs[0].chan=4; limbs[0].band=0; limbs[0].anchor=0; limbs[0].cum=false; limbs[0].crate=1; limbs[0].creflect=0;
  limbs[0].type=0; limbs[0].negative=false;
  limbs[0].aisoff=false; limbs[0].aoff=0; limbs[0].ascale=0; limbs[0].aspring=0;
  limbs[0].length=0; limbs[0].lmin=0;
  limbs[0].x=0; limbs[0].y=0; limbs[0].x0=0; limbs[0].y0=0;
  limbs[0].ang0=0; limbs[0].ang=0; limbs[0].ang1=0; limbs[0].f=0;
  nlimbs=1;
  MakeChildren();
  //
  TShape s; s.limbs=true; shapes.push_back(s);
}




void TBody::MakeChildren()
{ // each limb has a number of children.
  // the primary way this is stored is through each limb having its 'root.
  // but this makes for slow tree traversal. Therefore, we cache
  // a generated-list of children of each limb. In the array 'children'.
  // Say that a limb has childindex=5. Then children[5,...] = {7,9,11,0}
  // to indicate that limbs 7,9,11 are children of our current limb.
  // Note: this array is guaranteed to be no larger than nlimbs*2.
  children.resize(nlimbs*2,-1);
  // build up the complete list
  vector<list<int> > cs; cs.resize(nlimbs);
  for (int n=1; n<nlimbs; n++)
  { int root = limbs[n].root;
    cs[root].push_back(n);
  }
  // flatten it
  int tot=0;
  for (int n=0; n<nlimbs; n++)
  { limbs[n].childi=tot;
    for (list<int>::const_iterator i=cs[n].begin(); i!=cs[n].end(); i++)
    { children[tot] = *i;
      tot++;
    }
    children[tot]=0;
    tot++;
  }
}




bool StylesFromString(const char *buf,list<TStyle> &styles)
{ styles.clear();
  const char *c=buf, *end=c; while (*end!='\r' && *end!='\n' && *end!=0) end++;
  int numstyles=0; string nis(c,end-c); int i=sscanf(nis.c_str(),"%i",&numstyles);
  if (i!=1) return false;
  for (int i=0; i<numstyles; i++)
  { c=end; while (*c=='\r' || *c=='\n') c++; if (*c==0) return false; 
    end=c; while (*end!='\r' && *end!='\n' && *end!=0) end++;
    TStyle s; s.fromstring(string(c,end-c)); if (s.name!="") styles.push_back(s);
  }
  return true;
}

unsigned int StylesToString(char *buf,unsigned int blen,list<TStyle> &styles)
{ string s=StringInt((int)styles.size())+"\r\n";
  for (list<TStyle>::const_iterator y=styles.begin(); y!=styles.end(); y++) s += y->tostring()+"\r\n";
  if (s.length()+1>blen) return (int)s.length()+1;
  memcpy(buf,s.c_str(),s.length()); buf[s.length()]=0;
  return (int)s.length()+1;
}


void MakeBindexes(TBody *body)
{ sort(body->bmps.begin(),body->bmps.end());
  map<string,int> bmpmap;
  for (int i=0; i<(int)body->bmps.size(); i++)
  { bmpmap.insert(pair<const string,int>(StringLower(body->bmps[i].name),i));
  }
  for (vector<TShape>::iterator i=body->shapes.begin(); i!=body->shapes.end(); i++)
  { TColor &brush = i->brush;
    brush.bindex=-1;
    if (!i->limbs && (brush.type==ctBitmap || brush.dtype==ctBitmap)) 
    { map<string,int>::const_iterator index = bmpmap.find(StringLower(brush.bitmap));
      if (index!=bmpmap.end()) brush.bindex=index->second;
      else {string err="Couldn't find "+brush.bitmap; LUASSERTMM(err.c_str());
      }
    }
  }
  for (vector<TEffect>::iterator i=body->effects.begin(); i!=body->effects.end(); i++)
  { for (vector<TColor>::iterator j=i->cols.begin(); j!=i->cols.end(); j++)
    { j->bindex=-1;
      if (j->type==ctBitmap || j->dtype==ctBitmap)
      { map<string,int>::const_iterator index = bmpmap.find(StringLower(j->bitmap));
        LUASSERTM(index!=bmpmap.end(),("Couldn't find "+j->bitmap).c_str());
        if (index!=bmpmap.end()) j->bindex=index->second;
      }
    }
  }
}



void MakeEindexes(TBody *body)
{ map<string,int> emap;
  for (int i=0; i<(int)body->effects.size(); i++)
  { emap.insert(pair<const string,int>(StringLower(body->effects[i].name),i));
  }
  //
  for (vector<TShape>::iterator i=body->shapes.begin(); i!=body->shapes.end(); i++)
  { i->brush.eindex=-1;
    if (!i->limbs && i->brush.type==ctEffect) 
    { map<string,int>::const_iterator index = emap.find(StringLower(i->brush.effect));
      LUASSERTM(index!=emap.end(),("Couldn't find "+i->brush.effect).c_str());
      i->brush.eindex=index->second; i->brush.ef=body->effects[i->brush.eindex].f;
      i->brush.ereflect=body->effects[i->brush.eindex].creflect;
    }
    if (!i->limbs && i->line.type==ctEffect)
    { map<string,int>::const_iterator index = emap.find(StringLower(i->line.effect));
      LUASSERTM(index!=emap.end(),("Couldn't find "+i->line.effect).c_str());
      i->line.eindex=index->second; i->line.ef=body->effects[i->line.eindex].f;
      i->line.ereflect=body->effects[i->line.eindex].creflect;
    }
  }
  for (int i=0; i<body->nlimbs; i++)
  { body->limbs[i].color.eindex=-1;
    if (body->limbs[i].color.type==ctEffect)
    { map<string,int>::const_iterator index = emap.find(StringLower(body->limbs[i].color.effect));
      LUASSERTM(index!=emap.end(),("Couldn't find "+body->limbs[i].color.effect).c_str());
      body->limbs[i].color.eindex=index->second; body->limbs[i].color.ef=body->effects[body->limbs[i].color.eindex].f;
      body->limbs[i].color.ereflect=body->effects[body->limbs[i].color.eindex].creflect;
    }
  }
}



TBody* TBody::LoadBody(void *pbuf,unsigned int bufsize, char *err, bool strict)
{ LUASSERT(err!=0);
  *err=0;
  char *buf = (char*)pbuf;
  // First, a quick check: check the first thousand bytes for "\nversion=" and
  // see if the version is high enough. If it is, we will load it as a zip. If not, not.
  if (bufsize<=10) {strcpy(err,"File too small"); return 0;}
  unsigned long magic = * ((unsigned long*)buf);
  bool iszipfile = (magic==0x04034b50);
  bool isstickfile = (magic==0x6d696c6e);
  if (!iszipfile && !isstickfile) {strcpy(err,"File lacks stk magic bytes"); return 0;}
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
  if (isstickfile)
  { TBody *b = new TBody();
    bool res = b->ReadData(buf,err,rdOverwrite,NULL);
    if (!res) {delete b; LUASSERT(*err!=0); return 0;}
    LUASSERT(*err==0);
    return b;
  }
  //
  // otherwise, it was a new version, zipped up, maybe with a zip-sfx header
  // now we can do the zip
  TBody *body = new TBody();
  HZIP hz = OpenZip(buf,bufsize,0);
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
      if (strict) rflags=(ReadDataFlags)(rflags|rdStrict);
      allok &= body->ReadData(buf,err,rflags,NULL);
      delete[] buf;
    }
    else if (ext==".bmp" || ext==".jpg" || ext==".jpeg")
    { TBmp bmp;
      bmp.buf = new char[ze.unc_size];
      bmp.bufsize = ze.unc_size;
      bmp.name = ChangeFileExt(ze.name,"");
      bmp.fn = ze.name;
      UnzipItem(hz,i, bmp.buf,bmp.bufsize);
      body->bmps.push_back(bmp);
    }
    else if (StringLower(ze.name)=="styles.txt")
    { char *buf = new char[ze.unc_size+1];
      UnzipItem(hz,i, buf,ze.unc_size);
      buf[ze.unc_size]=0;
      bool res = StylesFromString(buf,body->styles);
      allok &= res;
      if (!res) sprintf(err,"invalid styles");
      delete[] buf;
    }
    // we just ignore anything we don't yet know:
  }
  CloseZip(hz);
  if (!allok) {delete body; LUASSERT(*err!=0); return 0;}
  // fix up bitmap,effect information
  MakeBindexes(body);
  MakeEindexes(body);
  LUASSERT(*err==0);
  return body;
}

// strip: turn bitmaps into native
// remove style information


//--------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------








string StringInt(int i) {char c[20]; sprintf(c,"%ld",i); return string(c);}
string StringFloat(double d) {char c[30]; sprintf(c,"%f",d); return string(c);}
string StringTrim(const string s)
{ size_t first=s.find_first_not_of(" \r\n\t"); if (first==string::npos) return "";
  size_t last = s.find_last_not_of(" \r\n\t");
  return s.substr(first, last-first+1);
}
string StringLower(const string s) {string t=s; for (unsigned int i=0; i<t.length(); i++) t[i] = (char)tolower(t[i]); return t;}
int LastDelimiter(const string Delimiters, const string s)
{ if (s=="") return 0;
  for (int i=(int)s.length()-1; i>=0; i--) {if (Delimiters.find(s[i])!=string::npos) return (int)i;}
  return 0;
}
string ChangeFileExt(const string FileName, const string Extension) {int i=LastDelimiter(".\\/:",FileName); if (i==0 || FileName[i]!='.') return FileName+Extension; else return FileName.substr(0,i)+Extension;}
string ExtractFileExt(const string FileName) {int i=LastDelimiter(".\\/:",FileName); if (i==0 || FileName[i]!='.') return ""; return FileName.substr(i);}
string ExtractFileName(const string FileName) {int i=LastDelimiter("/\\:",FileName); if (i==0) return FileName; else return FileName.substr(i+1);}

string StringUpper(const string s) {string t(s); for (unsigned int i=0; i<t.length(); i++) t[i] = (char)toupper(t[i]); return t;}
string ExtractFilePath(const string FileName) {int i=LastDelimiter("\\/:",FileName); return FileName.substr(0,i);}
string ExcludeTrailingBackslash(const string s) {if (s.length()==0) return s; if (s[s.length()-1]=='\\' || s[s.length()-1]=='/') return s.substr(0,s.length()-1); return s;}
bool DirectoryExists(const string d)
{ DWORD code=GetFileAttributes(d.c_str()); if (code==0xFFFFFFFF) return false;  if ((code&FILE_ATTRIBUTE_DIRECTORY)==FILE_ATTRIBUTE_DIRECTORY) return true;
  return false;
}
// ForceDirectories: ensures that a directory path exists.
bool ForceDirectories(const string dir)
{ string d = dir;
  if (d.length()==0) return false;
  d = ExcludeTrailingBackslash(dir);
  if (d.length()<3 || DirectoryExists(d) || ExtractFilePath(d)==d) return true;  // avoid 'xyz:\' problem.
  bool res = ForceDirectories(ExtractFilePath(dir));
  if (!res) return false;
  BOOL br = CreateDirectory(dir.c_str(),NULL);
  if (br) return true; else return false;
}
// IsFilePathRelative: boolean. Does what it says :)
bool IsFilePathRelative(const string fn)
{ string path = ExtractFilePath(fn);
  if (path=="") return false;
  // an absolute path starts with \ or / or c:\ or whatever.
  if (path[0]=='\\') return false;
  if (path[0]=='/') return false;
  if (path.length()>=3 && path[1]==':') return false;
  // otherwise it has a path that doesn't start with one of the baddies,
  // hence it must be relative
  return true;
}






} // namespace

