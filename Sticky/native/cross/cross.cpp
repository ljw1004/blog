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
#include <math.h>
#include "../body.h"
using namespace stk;
#include <gd.h>  // nb. borland needs #define WIN32 for gd.h to work correctly
void stk::luassertmsg(const char *s,const char *msg,const char *f,unsigned int l) {printf("%s '%s' - %s:%u\n",msg,s,f,l);}



typedef struct {double anchx;double anchy;double scale;int soffx;int soffy;} Dat2b;

int g2b2x(void *dat,double x)
{ Dat2b *bd = (Dat2b*)dat; if (bd==0) return 0;
  return (int)((x+bd->anchx+5)*bd->scale+0.5)+bd->soffx;
}
int g2b2y(void *dat,double y)
{ Dat2b *bd = (Dat2b*)dat; if (bd==0) return 0;
  return (int)((y+bd->anchy+5)*bd->scale+0.5)+bd->soffy;
}


#pragma pack(2)
struct TBitmapFileHeader
{ unsigned short bfType;
  unsigned long  bfSize;
  unsigned short bfReserved1, bfReserved2;
  unsigned long  bfOffBits;
};
struct TBitmapInfoHeader
{ unsigned long biSize;
  long biWidth;
  long biHeight;
  unsigned short biPlanes;
  unsigned short biBitCount;
  unsigned long  biCompression;
  unsigned long  biSizeImage;
  long biXPelsPerMeter;
  long biYPelsPerMeter;
  unsigned long biClrUsed;
  unsigned long biClrImportant;
};
#pragma pack(4)
struct TRgbQuad {unsigned char b,g,r,x;};


gdImage *gdImageCreateFromBmpPtr(int size, void *data)
{ LUASSERTM(sizeof(TBitmapFileHeader)==14 && sizeof(TBitmapInfoHeader)==40 && sizeof(TRgbQuad)==4, "Bitmap file structures are the wrong size");
  if (size<sizeof(TBitmapFileHeader)+sizeof(TBitmapInfoHeader)) return 0;
  TBitmapFileHeader *bfh=(TBitmapFileHeader*)data;
  TBitmapInfoHeader *bih=(TBitmapInfoHeader*)(((char*)data)+sizeof(TBitmapFileHeader));
  TRgbQuad *bmiColors=(TRgbQuad*)(((char*)bih)+sizeof(TBitmapInfoHeader));
  int ncols=bih->biClrUsed; if (ncols==0) ncols=1 << bih->biBitCount;
  unsigned char *bits = (unsigned char*)data + bfh->bfOffBits;
  int width=bih->biWidth, height=bih->biHeight;
  bool topdown=false; if (height<0) {topdown=true; height=-height;}
  int widthbytes = (((width*bih->biBitCount)+30)&~31)>>3;
  int totbytes = widthbytes*height;
  if ((size_t)size<bfh->bfOffBits+totbytes) return 0;
  gdImage *im = gdImageCreateTrueColor(width,height);
  LUASSERTM(bih->biPlanes==1, "Bitmap has planes!=1");
  LUASSERTM(bih->biCompression==0, "Bitmap has compression");
  if (bih->biBitCount==8)
  { for (int y=0; y<height; y++)
    { unsigned char *rowbits = bits+widthbytes* (topdown?y:(height-y-1));
      for (int x=0; x<width; x++)
      { int i=rowbits[x]; int r=bmiColors[i].r, g=bmiColors[i].g, b=bmiColors[i].b;
        int col = gdTrueColor(r,g,b);
        gdImageSetPixel(im,x,y,col);
      }
    }
  }
  else if (bih->biBitCount==24)
  { for (int y=0; y<height; y++)
    { unsigned char *rowbits = bits+widthbytes* (topdown?y:(height-y-1));
      for (int x=0; x<width; x++)
      { int b=rowbits[x*3+0], g=rowbits[x*3+1], r=rowbits[x*3+2];
        int col = gdTrueColor(r,g,b);
        gdImageSetPixel(im,x,y,col);
      }
    }
  }
  else if (bih->biBitCount==32)
  { for (int y=0; y<height; y++)
    { unsigned char *rowbits = bits+widthbytes* (topdown?y:(height-y-1));
      for (int x=0; x<width; x++)
      { int b=rowbits[x*4+0], g=rowbits[x*4+1], r=rowbits[x*4+2];
        int col = gdTrueColor(r,g,b);
        gdImageSetPixel(im,x,y,col);
      }
    }
  }
  else if (bih->biBitCount==1)
  { for (int y=0; y<height; y++)
    { unsigned char *rowbits = bits+widthbytes* (topdown?y:(height-y-1));
      for (int x=0; x<width; x++)
      { int i=rowbits[x/8];
        int bit=7-(x%8); i=i>>bit; i=i&0x1;
        int r=bmiColors[i].r, g=bmiColors[i].g, b=bmiColors[i].b;
        int col = gdTrueColor(r,g,b);
        gdImageSetPixel(im,x,y,col);
      }
    }
  }
  else if (bih->biBitCount==4)
  { for (int y=0; y<height; y++)
    { unsigned char *rowbits = bits+widthbytes* (topdown?y:(height-y-1));
      for (int x=0; x<width; x++)
      { int i=rowbits[x/2]; if ((x%2)==1) i=i>>4; i=i&0xF;
        int r=bmiColors[i].r, g=bmiColors[i].g, b=bmiColors[i].b;
        int col = gdTrueColor(r,g,b);
        gdImageSetPixel(im,x,y,col);
      }
    }
  }
  else LUASSERTMM("Bitmap has !=8,!=24 bpp");
  return im;
}


struct TBmpKey
{ string name; int width; int height; // these are the screen width and height in pixels of the rendered image
  bool operator==(const TBmpKey &k) const {return name==k.name && width==k.width && height==k.height;}
  bool operator<(const TBmpKey &k) const {if (name<k.name) return true; if (name>k.name) return false; if (width<k.width) return true; if (width>k.width) return false; return (height<k.height);}
};
struct TBmpValue
{ gdImage *im;
};

map<TBmpKey, TBmpValue> bmphash;

void FlushImages()
{ for (map<TBmpKey,TBmpValue>::iterator i = bmphash.begin(); i!=bmphash.end(); i++)
  { TBmpValue &val = i->second;
    if (val.im!=0) gdImageDestroy(val.im); val.im=0;
  }
  bmphash.clear();
}

bool LoadImages(TBody *body, char *err)
{ for (unsigned int nbmp=0; nbmp<body->bmps.size(); nbmp++)
  { TBmp &bmp = body->bmps[nbmp];
    bool hastransparency=true;
    if (bmp.name.size()>=3 && StringLower(bmp.name.substr(bmp.name.size()-3))=="-nt") hastransparency=false;
    bool isbmp = (bmp.buf[0]=='B' && bmp.buf[1]=='M');
    gdImage *im=0;
    if (isbmp) im = gdImageCreateFromBmpPtr(bmp.bufsize,bmp.buf);
    else im = gdImageCreateFromJpegPtr(bmp.bufsize,bmp.buf);
    if (im==0) {FlushImages(); strcpy(err,bmp.name.c_str()); return false;}
    bmp.bwidth = gdImageSX(im); bmp.bheight = gdImageSY(im);
    if (hastransparency)
    { int ctrans = gdImageGetPixel(im, 0,0); // use the top left pixel as transparent
      gdImageColorTransparent(im,ctrans);
    }
    TBmpKey key; key.name=bmp.name; key.width=bmp.bwidth; key.height=bmp.bheight;
    TBmpValue val; val.im=im;
    map<TBmpKey,TBmpValue>::iterator i = bmphash.find(key);
    if (i!=bmphash.end()) bmphash.erase(i);
    bmphash.insert(pair<TBmpKey,TBmpValue>(key,val));
  }
  return true;
}



class TEffPt
{ gdPoint stockpt[1000]; int stockpi[1000];
  public:
  gdPoint *pt; int *pi; int ptlen;
  TEffPt() : pt(stockpt),pi(stockpi),ptlen(1000) {}
  ~TEffPt() {if (pt!=stockpt) delete[] pt; if (pi!=stockpi) delete[] pi;}
  void ensure(int asize)
  { if (asize<ptlen) return;
    int newlen=2*asize; gdPoint *ptnew=new gdPoint[newlen]; int *pinew=new int[newlen];
    memcpy(ptnew,pt,sizeof(gdPoint)*ptlen); memcpy(pinew,pi,sizeof(int)*ptlen);
    if (pt!=stockpt) delete[] pt; if (pi!=stockpi) delete[] pi;
    pt=ptnew; pi=pinew; ptlen=newlen;
  }
  int add(int optpos,int x,int y,int id)
  { ensure(optpos+1); pt[optpos].x=x; pt[optpos].y=y; pi[optpos]=id; return optpos+1;
  }
};

typedef int (*b2func)(void *dat,double b);


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
  double mul=1+log(perim/50.0)/log(2.0); if (mul<1) mul=1;
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



bool SimpleDraw(gdImage *im,int left,int top,int width,int height,TBody *body, TRgbQuad *bg, string *err=0)
{ bool fail=false;
  // The body has dimensions -5 to +5, and we add anchx/anchy to the body's coordinates.
  // This 10x10 square must be mapped onto the largest square inside rc.
  // We do this as screenx = (bodyx+anchx+5)*scale+soffset
  RGBCOLOR back=body->limbs[0].color.rgb;
  int clrForeground=gdTrueColor(255,255,255);
  int clrBackground=gdTrueColor(back.r,back.g,back.b);
  if (bg!=0) {bg->r=back.r; bg->g=back.g; bg->b=back.b;}
  gdImageColorTransparent(im,clrBackground);
  //
  double scale; int soffx,soffy;
  if (width>height) {soffx=left+(width-height)/2; soffy=top+0; scale=((double)height)/10.0;}
  else {soffx=left+0; soffy=top+(height-width)/2; scale=width/10;}
  Dat2b dat; dat.anchx=body->anchx; dat.anchy=body->anchy; dat.scale=scale; dat.soffx=soffx; dat.soffy=soffy;
  //
  gdImageFilledRectangle(im,left,top,left+width,top+height,clrBackground);
  TEffPt ept;
  //
  for (int s=0; s<(int)body->shapes.size(); s++)
  { TShape &shape = body->shapes[s];
    if (!shape.limbs)
    { //SetPolyFillMode(hdc,shape.balternate?ALTERNATE:WINDING);
      bool simplecircle = (shape.p.size()==2 && shape.p[0].i==shape.p[1].i && shape.p[0].ar!=shape.p[1].ar && body->limbs[shape.p[0].i].type==3);
      if (shape.brush.dtype==ctBitmap && (shape.brush.bindex<0 || shape.brush.bindex>=(int)body->bmps.size())) {fail=true; if (err!=0) *err="bindex";}
      if (simplecircle && shape.brush.dtype==ctBitmap && shape.brush.bindex!=-1)
      { // a bitmap, center cx,cy, angle ang.
        TLimb &limb = body->limbs[shape.p[0].i];
        double dx=limb.x-limb.x0,dy=limb.y-limb.y0, rad=sqrt(dx*dx+dy*dy);
        int x0=g2b2x(&dat,limb.x0-rad), y0=g2b2y(&dat,limb.y0-rad), x1=g2b2x(&dat,limb.x0+rad), y1=g2b2y(&dat,limb.y0+rad);
        TBmp &bmp = body->bmps[shape.brush.bindex];
        // adjust the rect so the bitmap is kept proportional
        double f=((double)bmp.bwidth)/((double)bmp.bheight);
        if (f>1) {int cy=(y0+y1)/2, h=y1-y0; h=(int)(((double)h)/f); y0=cy-h/2; y1=cy+h/2;}
        else if (f<1) {int cx=(x0+x1)/2, w=x1-x0; w=(int)(((double)w)*f); x0=cx-w/2; x1=cx+w/2;}
        int cx=(x0+x1)/2, cy=(y0+y1)/2;
        //
        TBmpKey key; key.name=bmp.name; key.width=bmp.bwidth; key.height=bmp.bheight;
        map<TBmpKey,TBmpValue>::iterator i = bmphash.find(key);
        if (i!=bmphash.end() && i->second.im!=0)
        { gdImage *brush = i->second.im;
          // the brush is drawn a rectangle x0,y0,x1,y1 which is rotated "ang" about its center
          // (1) First, rotate it. In GD, CopyRotate doesn't respect transparency.
          int transp=gdImageGetTransparent(brush);
          if (transp==-1) transp=gdTrueColor(1,0,0); // really we need to pick a colour that didn't appear anywhere in the brush.
          int rwidth=max(bmp.bwidth,bmp.bheight)*3/2, rheight=rwidth;
          gdImage *rbrush = gdImageCreateTrueColor(rwidth,rheight);
          gdImageColorTransparent(rbrush,transp);
          gdImageFilledRectangle(rbrush,0,0,rwidth,rheight,transp);
          gdImageCopyRotated(rbrush,brush, rwidth/2,rheight/2, 0,0,bmp.bwidth,bmp.bheight, (int)(-limb.ang*180.0/pi));
          // (2) Then resize/copy onto the main image. In GD, CopyResized does respect transparency
          double scale=((double)(x1-x0))/((double)bmp.bwidth); // assert: (y1-y0)/bheight is the same.
          int swidth = (int)(((double)rwidth)*scale), sheight = (int)(((double)rheight)*scale);
          gdImageCopyResized(im,rbrush, cx-swidth/2,cy-sheight/2,0,0, swidth,sheight, rwidth,rheight);
          gdImageDestroy(rbrush);
        }
      }
      if (simplecircle)
      { TLimb &limb = body->limbs[shape.p[0].i];
        double dx=limb.x-limb.x0,dy=limb.y-limb.y0, rad=sqrt(dx*dx+dy*dy);
        int x0=g2b2x(&dat,limb.x0-rad), y0=g2b2y(&dat,limb.y0-rad), x1=g2b2x(&dat,limb.x0+rad), y1=g2b2y(&dat,limb.y0+rad);
        //
        int linecol=-1;
        if (shape.line.dtype==ctNone) {}
        else if (shape.line.dtype==ctRGB) linecol = gdTrueColor(shape.line.rgb.r, shape.line.rgb.g, shape.line.rgb.b);
        else {fail=true; if (err!=0) *err="shape line colour";}
        if (linecol!=-1) gdImageSetThickness(im, (int)(0.03*scale*shape.thickness+0.5));
        //
        int fillcol=-1;
        if (shape.brush.dtype==ctNone) {}
        else if (shape.brush.dtype==ctDefault) fillcol=clrForeground;
        else if (shape.brush.dtype==ctRGB) fillcol=gdTrueColor(shape.brush.rgb.r, shape.brush.rgb.g, shape.brush.rgb.b);
        else if (shape.brush.dtype==ctBitmap) fillcol=-1;
        else {fail=true; if (err!=0) *err="shape brush colour";}
        if (fillcol!=-1) gdImageFilledEllipse(im, (x0+x1)/2,(y0+y1)/2, x1-x0,y1-y0, fillcol);
        if (linecol!=-1) gdImageArc(im, (x0+x1)/2,(y0+y1)/2, x1-x0,y1-y0, 0,360, linecol);
      }
      else
      { // not a simple circle, so we do it the long way...,
        // first, build the pt[] array.
        int ptpos=0; for (int si=0; si<(int)shape.p.size(); si++)
        { TJointRef j0=shape.p[si], j1=shape.p[(si+1)%shape.p.size()];
          TLimb &limb0=body->limbs[j0.i]; 
          if (j0.i==j1.i && j0.ar!=j1.ar && (limb0.type==1 || limb0.type==3)) ptpos=add_arc(g2b2x,g2b2y,&dat,&ept,ptpos,limb0,j0.ar,-1);
          else ptpos=add_pt(g2b2x,g2b2y,&dat,&ept,ptpos,limb0,j0.ar,-1);
        }
        if (shape.p.size()>2) ptpos=ept.add(ptpos,ept.pt[0].x,ept.pt[0].y,(int)shape.p.size()-1); // to close it!      
        int fillcol=-1;
        if (shape.brush.dtype==ctNone) {}
        else if (shape.brush.dtype==ctDefault) fillcol=clrForeground;
        else if (shape.brush.dtype==ctRGB) fillcol=gdTrueColor(shape.brush.rgb.r, shape.brush.rgb.g, shape.brush.rgb.b);
        else {fail=true; if (err!=0) *err="brush colour";}
        int linecol=-1;
        if (shape.line.dtype==ctNone) {}
        else if (shape.line.dtype==ctRGB) linecol=gdTrueColor(shape.line.rgb.r, shape.line.rgb.g, shape.line.rgb.b);
        else {fail=true; if (err!=0) *err="shape line colour";}
        if (linecol!=-1) gdImageSetThickness(im, (int)(0.03*scale*shape.thickness+0.5));
        if (fillcol!=-1 && ptpos>2) gdImageFilledPolygon(im,ept.pt,ptpos-1,fillcol);
        if (linecol!=-1) gdImageOpenPolygon(im,ept.pt,ptpos,linecol);
      }
    }
    else if (shape.limbs)
    { for (int n=1; n<body->nlimbs; n++)
      { TLimb &limb=body->limbs[n];
        int lx0 = (int)((limb.x0+body->anchx+5)*scale+0.5)+soffx;
        int ly0 = (int)((limb.y0+body->anchy+5)*scale+0.5)+soffy;
        int lx1 = (int)((limb.x +body->anchx+5)*scale+0.5)+soffx;
        int ly1 = (int)((limb.y +body->anchy+5)*scale+0.5)+soffy;
        if (limb.color.dtype!=ctNone)
        { if (limb.color.dtype!=ctRGB && limb.color.dtype!=ctDefault) {fail=true; if (err!=0) *err="line colour";}
          int r=limb.color.rgb.r, g=limb.color.rgb.g, b=limb.color.rgb.b;
          int limbcol=clrForeground;
          if (limb.color.dtype!=ctDefault) limbcol=gdTrueColor(r,g,b);
          gdImageSetThickness(im, (int)(0.03*scale*limb.thickness+0.5));
          if (limb.type==0 || limb.type==2) // a line
          { gdImageLine(im, lx0, ly0, lx1, ly1, limbcol);
          }
          else if (limb.type==1) // an arc. Origin at limb->x0y0. Radius=limb->length. Ends at limb->ang0,1
          { double d=scale*limb.length;
            double angA,angB; if (limb.ascale<0) {angA=limb.ang0;angB=limb.ang;} else {angA=limb.ang; angB=limb.ang0;}
            int xA=lx0+(int)(d*cos(angA)), yA=ly0+(int)(d*sin(angA));
            int xB=lx0+(int)(d*cos(angB)), yB=ly0+(int)(d*sin(angB));
            int ddAB=(xB-xA)*(xB-xA)+(yB-yA)*(yB-yA);
            double f1=angA*180.0/pi, f2=angB*180.0/pi; while (f1<0) f1+=360; while (f2<0) f2+=360;
            if (ddAB<100 && angA-angB<pi) gdImageLine(im, xA,yA, xB,yB, limbcol); // draw a line for things less than ten pixels
            else gdImageArc(im, lx0,ly0, (int)(d*2.00),(int)(d*2.0), (int)f2, (int)f1, limbcol); 
          }
          else if (limb.type==3) // a circle. Origin at limb->x0y0. Radius according to limb.x,limb.y
          { double dx=limb.x-limb.x0,dy=limb.y-limb.y0, d=scale*sqrt(dx*dx+dy*dy); int id=(int)d;
            gdImageArc(im, lx0,ly0, id*2,id*2, 0,360, limbcol);
          }
        }
      }
    }
  }

  return !fail;
}


int main(int argc, char *argv[])
{ if (sizeof(TBitmapFileHeader)!=14 || sizeof(TBitmapInfoHeader)!=40 || sizeof(TRgbQuad)!=4)
  { string s = string("sizeof(TBitmapFileHeader)=")+StringInt(sizeof(TBitmapFileHeader))+" (should be 14)\n"
               "sizeof(TBitmapInfoHeader)="+StringInt(sizeof(TBitmapInfoHeader))+" (should be 40)\n"
               "sizeof(TRgbQuad)="+StringInt(sizeof(TRgbQuad))+" (should be 4)\n";
    LUASSERTMM(s.c_str());
    return 1;
  }
  //
  const char *fnstk=0, *fngif=0;
  if (argc==2) fnstk=argv[1];
  else if (argc==4 && (strncmp(argv[1],"-gif",4)==0 || strncmp(argv[1],"--gif",5)==0)) {fngif=argv[2]; fnstk=argv[3];}
  else if (argc==4 && (strncmp(argv[2],"-gif",4)==0 || strncmp(argv[2],"--gif",5)==0)) {fnstk=argv[1]; fngif=argv[3];}
  if (fnstk==0)
  { string fnexe=ExtractFileName(argv[0]);
    printf("%s: tests a stickfile and produces a gif preview.\nUsage: %s \"file.stk\" [-gif \"file.gif\"]\n",fnexe.c_str(),fnexe.c_str());
    return 1;
  }
  //
  FILE *f=fopen(fnstk,"rb"); if (f==0) {printf("%s not found\n",fnstk); return 1;}
  fseek(f,0,SEEK_END); int size=ftell(f); fseek(f,0,SEEK_SET);
  char *buf = new char[size+1]; fread(buf,1,size,f); fclose(f);
  buf[size]=0; char err[1024];
  stk::TBody *b = stk::TBody::LoadBody(buf,size,err,true);
  delete[] buf;
  if (b==0) {printf("%s\n",err); return 1;}
  //
  
  // If all we had to do was test for consistency, then we're done...
  if (fngif==0) {delete b; return 0;}

  // Otherwise, we produce a gif.
  bool res = LoadImages(b,err);
  if (!res) {LUASSERT(*err!=0); printf("problem with images - %s\n",err); delete b; return 1;}
  double freq[3][6];
  for (int chan=0; chan<3; chan++)
  { for (int band=0; band<6; band++)
    { freq[chan][band] = ((double)(rand()%1000))/1000.0;
    }
  }
  b->AssignFreq(freq,1.0);
  b->RecalcEffects(true);
  b->Recalc();
  gdImage *imbig = gdImageCreateTrueColor(512,512);
  TRgbQuad bg; string serr;
  res=SimpleDraw(imbig,0,0,512,512,b,&bg,&serr); int transp=gdTrueColor(bg.r,bg.g,bg.b);
  if (!res) {gdImageDestroy(imbig); FlushImages(); delete b; printf("%s\n",serr.c_str()); return 1;}
  gdImage *im = gdImageCreateTrueColor(128,128);
  gdImageRectangle(im,0,0,128,128,transp); gdImageColorTransparent(im,transp);
  gdImageCopyResampled(im,imbig,0,0,0,0, 128,128,512,512);
  gdImageDestroy(imbig);
  gdImageTrueColorToPalette(im,false,256);
  int fsize; void *fbuf=gdImageGifPtr(im,&fsize);
  f=fopen(fngif,"wb"); if (f==0) {printf("%s not writable\n",fngif); gdFree(fbuf); gdImageDestroy(im); delete b; FlushImages(); return 1;}
  fwrite(fbuf,1,fsize,f); fclose(f);
  gdFree(fbuf);
  gdImageDestroy(im);
  delete b;
  FlushImages();
  printf("bg=(%i,%i,%i)\n",(int)bg.r,(int)bg.g,(int)bg.b);
  return 0;
}

