#include <windows.h>
#include <commdlg.h>
#include <commctrl.h>
#include <shellapi.h>
#pragma warning( push )
#pragma warning( disable: 4201 )
#include <mmsystem.h>
#pragma warning( pop )
#include <string>
#include <list>
#include <vector>
using namespace std;
#include <math.h>
#include <stdlib.h>
#include <stdio.h>
#include "QCDModDefs.h"
#include "QCDModVisuals.h"
#include "../body.h"
#include "../utils.h"
using namespace stk;


HINSTANCE hInstance;

list<string> recents;  // the recent music we have heard

void stk::luassertmsg(const char *s,const char *msg,const char *f,unsigned int l)
{ char c[2000]; wsprintf(c,"%s '%s' - %s:%u\r\n",msg,s,f,l);
  OutputDebugString(c);
}


// Calls from the Player
static void Idle(void);
static int  Render(void);
static int  Event(PluginEventOp type, long x, long y);
static void About(void);
static void Configure(long flags);
static void Wrap(void);



PluginInfo* info;
const int nresonators=12;


void Idle() {}
int Event(PluginEventOp type, long x, long y) {return 0;} // For pointer, clicks, events. return 1 to update buffer.
void About() {}
void Configure(long flags) {}
void Wrap() {} // cleanup and deallocate

int Render()
{ for (int x=0; x<info->buffer->xSize; x++)
  { for (int y=0; y<info->buffer->ySize; y++)
    { info->buffer->rows[y][x].l=0;
    }
  }
  char c[1024];
  
  for (int i=0; i<nresonators*4; i++)
  { for (int s=0; s<2; s++)
    { int x=i; if (s==1) x=info->buffer->xSize-i-1;
      int ei=i/4;
      double e=info->energy[s][ei];
      e = 1 + 10*log10(e);
      e = e/-90;
      e *= info->buffer->ySize;
      int ey=(int)e; if (ey<0) ey=0; if (ey>=info->buffer->ySize) ey=info->buffer->ySize-1;
      for (int y=0; y<ey; y++)
      { info->buffer->rows[info->buffer->ySize-y-1][x].l=RGB(255,128,0);
      }
    }
  }
  return 1;
} 


PLUGIN_API int VISUALDLL_ENTRY_POINT(0)(PluginInfo *pluginInfo, QCDModInfo *modInfo, int fullinit)
{ if (modInfo!=0) modInfo->moduleString = "Dancing Stick Figures";
	if (pluginInfo!=0)
	{ pluginInfo->version = PLUGIN_API_VERSION;
		pluginInfo->about = About;
		pluginInfo->configure = Configure;
		pluginInfo->event = Event;
		pluginInfo->render = Render;
		pluginInfo->idle = Idle;
		pluginInfo->wrap = Wrap;
    // Keep a pointer to the system's struct
		info = pluginInfo;
  }
  if (pluginInfo!=0 && fullinit) // if fullinit, we have to fill out everything.
  { pluginInfo->triggerMode   = noTrigger;
		pluginInfo->triggerForm   = 0;
		pluginInfo->resonatorMode = stereoResonators;
		pluginInfo->resonatorForm = nresonators;
		pluginInfo->vuMeterMode   = 0;
		pluginInfo->ppMeterMode   = 0;
	}
  return TRUE;
}


