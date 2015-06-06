//-----------------------------------------------------------------------------
//
// File:	QCDVisualsDLL.cpp
//
// About:	See QCDVisualsDLL.h
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

#include "QCDVisualsDLL.h"

static PluginInfo*			info;

// TODO : static ResonatorInfo		resBank;

//-----------------------------------------------------------------------------

//
// If there are multiple visualizations, each will have its own entry point
// as VISUALDLL_ENTRY_POINT(n), where n is the index of the visualization
// in your visualization pack. If there is to be only one visualization,
// this line need never change.
// 

PLUGIN_API int VISUALDLL_ENTRY_POINT(0)(PluginInfo *pluginInfo, QCDModInfo *modInfo, int fullinit)
{
	//
	// Visual plugin initialization will be called to obtain basic information
	// (moduleString, version, about and configure) when fullinit == FALSE.
	// When fullinit == TRUE, perform a full initialization required to launch
	// visualization.
	//

	if (modInfo)
		modInfo->moduleString = "Sample Visualization Plugin v0.0";
	
	if (pluginInfo)
	{
		// Setup the the function pointers
		pluginInfo->version		= PLUGIN_API_VERSION;
		pluginInfo->about		= About;
		pluginInfo->configure	= Configure;
		pluginInfo->event		= Event;
		pluginInfo->render		= Render;
		pluginInfo->idle		= Idle;
		pluginInfo->wrap		= Wrap;

		// keep a copy of the pluginInfo struct since it will be the
		// means of getting analyzation data and player callbacks.
		// This is the one time it gets passed to the plugin.
		info = pluginInfo;

		if (fullinit) 
		{
			//
			// TODO : Set up analysis controls
			//
			// SAMPLE :
			// pluginInfo->triggerMode   = noTrigger;
			// pluginInfo->triggerForm   = 0;
			// pluginInfo->resonatorMode = customResonators;
			// pluginInfo->resonatorForm = (long)&resBank;
			// pluginInfo->vuMeterMode   = monoVUMeter;
			// pluginInfo->ppMeterMode   = 0;

			//
			// TODO : Set up resonator bank
			//
			// SAMPLE :
			// resBank.resonatorMode	= monoResonators;
			// resBank.resonatorForm	= resonators;
			// resBank.pitch			= RESONATORPITCH;
			// resBank.width			= RESONATORWIDTH;
			// resBank.fixedsampling	= !0;
			// resBank.decimation		= DECIMATION;
			// resBank.next				= 0;
		}
	}

	return !0;
}

//-----------------------------------------------------------------------------

static void Idle(void)
{
	//
	// TODO : Run background computations.
	//		  Use this low priority callback for secondary computations
	//
}

//-----------------------------------------------------------------------------

static int Render(void)
{
	//
	// TODO : Render the current frame into buffer.
	//		  Return 1 to have buffer rendered to player, 0 otherwise.
	//

	return 0;
}

//-----------------------------------------------------------------------------

static int Event(PluginEventOp type, long x, long y)
{
	//
	// TODO : Accept pointer motion, clicks and other player events.
	//		  Return 1 to have buffer updated to screen, 0 otherwise.
	//

	return 0;
}

//-----------------------------------------------------------------------------

static void About(void)
{
	//
	// TODO : Show the "about" dialog.
	//
}

//-----------------------------------------------------------------------------

static void Configure(long flags)
{
	//
	// TODO : Show the "configure" dialog.
	// flags param is for plugin menu support
	//
}

//-----------------------------------------------------------------------------

static void Wrap(void)
{
	//
	// TODO : Clean up and deallocate
	//
}

//-----------------------------------------------------------------------------

