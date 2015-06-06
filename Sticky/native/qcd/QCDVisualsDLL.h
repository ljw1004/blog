//-----------------------------------------------------------------------------
// 
// File:	QCDVisualsDLL.h
//
// About:	QCD Player Visualization module DLL interface.  For more 
//			documentation, see QCDModVisuals.h.
//
// Authors:	Written by Paul Quinn and Richard Carlson.
//
//	QCD multimedia player application Software Development Kit Release 1.0.
//
//	Copyright (C) 1997-2002 Quinnware
//
//	This code is free.  If you redistribute it in any form, leave this notice 
//	here.
//
//	This program is distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
//
//-----------------------------------------------------------------------------

#include "QCDModVisuals.h"

#ifndef QCDVISUALSDLL_H
#define QCDVISUALSDLL_H

// Calls from the Player
static void Idle(void);
static int  Render(void);
static int  Event(PluginEventOp type, long x, long y);
static void About(void);
static void Configure(long flags);
static void Wrap(void);

#endif //QCDVISUALSDLL_H