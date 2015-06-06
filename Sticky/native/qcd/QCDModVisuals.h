//-----------------------------------------------------------------------------
//
// File:	QCDModVisuals.h
//
// About:	Visualization plugin module interface.  This file is published with 
//			the visualization plugin SDK.
//
// Authors:	Written by David Levine.
//
// Copyright:
//
//	QCD multimedia player application Software Development Kit Release 1.0.
//
//	This code is free.  If you redistribute it in any form, leave this notice 
//	here.
//
//	This program is distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
//
//-----------------------------------------------------------------------------

#ifndef QCDMODVISUALS_H
#define QCDMODVISUALS_H

#include "QCDModDefs.h"

// name of the DLL export for input plugins
#define VISUALDLL_ENTRY_POINT(n)	QVisualModule##n

typedef union		{struct {unsigned char b, g, r, a;}; unsigned char v[4]; long l;} Pixel;

// The number of samples provided in the sound data
#define SAMPLES				2048	// 2K samples covers a single waveform at around 21.5 Hz.

// The maximum number of resonators for the spectral analysis.
// The resonators span the 20Hz to 20kHz frequency range, which is a 10 musical octave range,
// and are evenly distributed in portions of a musical octave.  If you use all 120 resonators,
// the resonators are spaced exactly one musical semitone apart, centered at standard pitches.
// Using 30 or 15 resonators yields a 1/3 octave or 2/3 octave width per resonator, yielding
// the spectral distribution of a standard pro-audio graphic equalizer.
#define MAXRESONATORS		120		// maximum number of resonators

//-----------------------------------------------------------------------------
// Event function, called by player when the following user-input and player 
// events for the Plugin are pending.
//-----------------------------------------------------------------------------
typedef enum 
{
	// user events
	pointerLeave = -1, 
	pointerMove = 1, 
	lbuttonDown = 2, 
	lbuttonUp = 3, 
	lbuttonDblClk = 6,
	rbuttonDblClk = 7,

	// player events
	trackChange = 10,
	trackInfoChange = 11
} PluginEventOp;

typedef	int		(*PluginEventFunc)(PluginEventOp type, long x, long y);

//-----------------------------------------------------------------------------
// Op functions, graphical services provided by the Player, called by Plugin:
//-----------------------------------------------------------------------------

// opFunctions provided by the Player
typedef enum 
{
	opGetMilliseconds,					// returns current system time in milliseconds
										// no parameters
	opBlitFromPict,						// retrieve named image resource and blit to coordinates
										// data = (RsrcInfo*), p0-p3 = UL and LR coordinates
	opComposite,						// apply transformation and RGBA composite using BlitInfo
										// data = (BlitInfo*)
	opMakeRaster,						// create a new Raster with specified coordinates
										// data = (Raster*), p0-p3 = UL and LR coordinates
	opFreeRaster,						// dispose of created raster
										// data = (Raster*)
	opFillRaster,						// fills given raster with the visual area skin graphic
										// data = (Raster*), p0-p3 = UL and LR coordinates relative to the visual area
	opMakeMap,							// create Map, returns: 0: error, -1: building, +1: done
										// data = (VolveInfo*), p0-p3 = UL and LR coordinates
	opFreeMap,							// dispose of Map
										// data = (VolveInfo*)
	opVolve								// run Volver using VolveInfo with associated Map
										// data = (VolveInfo*)
} PluginOp;

typedef	long	(*PluginOpFunc)(PluginOp op, void *data, long p0, long p1, long p2, long p3);

//-----------------------------------------------------------------------------
// Render function, called every 10 milliseconds (not reentrantly), with 
// current sound and analysis data.  New sound and analysis data may only be 
// available every Nth call depending on the OS.
//-----------------------------------------------------------------------------
typedef	int		(*PluginRenderFunc)(void);

//-----------------------------------------------------------------------------
// Resize function, called by player to notify of main buffer resize.
//-----------------------------------------------------------------------------
typedef	int		(*PluginResizeFunc)(long xSize, long ySize);

//-----------------------------------------------------------------------------
// Wrap function, called by player before unloading Plugin.
//-----------------------------------------------------------------------------
typedef	void	(*PluginWrapFunc)(void);

//-----------------------------------------------------------------------------
// Idle function, called by player at low priority for background computation.
//-----------------------------------------------------------------------------
typedef	void	(*PluginIdleFunc)(void);

//-----------------------------------------------------------------------------
// About function, called by player on user request for information about 
// Plugin.
//-----------------------------------------------------------------------------
typedef	void	(*PluginAboutFunc)(void);

//-----------------------------------------------------------------------------
// Configuration function, called by player on user request to configure 
// Plugin.
//-----------------------------------------------------------------------------
typedef	void	(*PluginConfigureFunc)(long flags);

//-----------------------------------------------------------------------------
// Format of platform-independent Raster structure:
// x0, y0 and x1, y1 define the upper-left and lower-right bounds of
// the active rectangle of the Raster, initialized to 0,0 and xSize,ySize
// alphaEnable is a boolean to tell the opComposite function whether to
// include this Raster's alpha channel in its transparency computations
// See Notes document for discussion.
//-----------------------------------------------------------------------------
typedef struct {
	void*				context;					// platform-specific data
	Pixel**				rows;						// row pointers
	long				xSize;						// buffer wide
	long				ySize;						// buffer high
	long				x0, y0, x1, y1;				// bounds: 0 <= n < nSize
	long				alphaEnable;				// transparency channel
} Raster;

//-----------------------------------------------------------------------------
// The PluginInfo structure, delivered to the Plugin as a pointer:
//-----------------------------------------------------------------------------
typedef struct 
{
	long				version;					// SDK version number [used in future release]
	long				size;						// size of this data structure [used in future release]

	void*				context;					// platform-specific data, set by Player

	PluginServiceFunc	serviceFunc;				// player services function, set by Player
	PluginOpFunc		opFunc;						// graphics operation service function, set by Player

	PluginEventFunc		event;						// set by Plugin, accept pointer motion and clicks
													//	(return 1 to have buffer updated to screen, 0 otherwise)
	PluginRenderFunc	render;						// set by Plugin, render the current frame into buffer
													//	(return 1 to render buffer, 0 otherwise)
	PluginResizeFunc	resize;						// set by Plugin, notify of main buffer resize 
													//	(return 1 if resize handled by plugin, 0 re-init plugin)
	PluginWrapFunc		wrap;						// set by Plugin, clean up an deallocate
	PluginIdleFunc		idle;						// set by Plugin, run background computations
	PluginAboutFunc		about;						// set by Plugin, run from plugin browser
	PluginConfigureFunc	configure;					// set by Plugin, run from plugin browser

	Raster*				buffer;						// set by Plugin, Plugin allocates extra Rasters
													// set by Player initially to the System Raster

	// analysis controls
	long				triggerMode;				// set by Plugin: none, mono, left, right
	long				triggerForm;				// set by Plugin: positive peak, negative peak
	long				resonatorMode;				// set by Plugin: none, mono, stereo, sum & dif
	long				resonatorForm;				// set by Plugin: number of resonators [1..120]
													// (ResonatorInfo*) for custom resonator banks
	long				vuMeterMode;				// set by Plugin: none, mono, stereo, sum & dif
	long				ppMeterMode;				// set by Plugin: none, mono, stereo, sum & dif

	// analysis output
	short				sound[2*SAMPLES];			// set by Player, interleaved channels
	double				energy[2][MAXRESONATORS];	// set by Player, resonator energy, 0.0..1.0
	double				vumeter[2];					// set by Player, VU level, 0.0..2.0, 0VU = 0.707
	double				ppmeter[2];					// set by Player, PP level, 0.0..1.0

} PluginInfo;


//-----------------------------------------------------------------------------
// Mode and Form Selectors
//-----------------------------------------------------------------------------
typedef enum
{
	// triggerMode: no triggering, trigger on sum, trigger on left, trigger on right
	noTrigger = 0, monoTrigger = 1, leftTrigger = 2, rightTrigger = 3,

	// triggerForm: positive peak, negative peak
	posTrigger = 0, negTrigger = 1,

	// resonatorMode: no resonators, one bank (L+R), two banks (L,R), two banks (L+R,L-R)
	noResonators = 0, monoResonators = 1, stereoResonators = 2, phaseResonators = 3,
	// customResonators: indicates a pointer to linked list of custom bank specifications in resonatorForm
	customResonators = 4,

	// resonatorForm: integer value from 1 through MAXRESONATORS (120)
	minResonators = 1, maxResonators = MAXRESONATORS,

	// vuMeterMode: no VU meters, one VU meter (L+R), two VU meters (L,R), two VU meters (L+R,L-R)
	noVUMeter = 0, monoVUMeter = 1, stereoVUMeter = 2, phaseVUMeter = 3,

	// ppMeterMode: no PP meters, one PP meter (L+R), two PP meters (L,R), two PP meters (L+R,L-R)
	noPPMeter = 0, monoPPMeter = 1, stereoPPMeter = 2, phasePPMeter = 3

} PluginSoundOp;

//-----------------------------------------------------------------------------
// Custom resonator bank specifier
//-----------------------------------------------------------------------------
typedef struct ResonatorInfo 
{
	struct ResonatorInfo*	next;					// pointer to info for the next bank, NULL if none

	PluginSoundOp			resonatorMode;			// see above
	long					resonatorForm;			// number of resonators in this bank
													// the total number of resonators in all banks is limited to 120.

	double					pitch;					// pitch of the first resonator in this bank in cycles per second
	double					width;					// width of each resonator in this bank in musical semitones
													// resonators are spaced at equal widths from the first resonator

	long					fixedsampling;			// false = autorange sampling rates, true = use fixed sampling rate
													// a resonator's pitch range is limited to its sampling rate / 4.41
													// autorange automatically selects an appropriate sampling rate for
													// each resonator in this bank, fixedsampling applies a fixed rate
													// to all resonators in this bank, specified by decimation factor:

	long					decimation;				// log base 2 of fixed decimation factor from 2x oversampling
													// 0 = 88.2 kHz sampling (ok for resonator pitches up to 20 kHz)
													// 1 = 44.1 kHz sampling (ok for resonator pitches up to 10 kHz)
													// 2 = 22.05 kHz sampling (ok for resonator pitches up to 5 kHz)
													// 3 = 11.025 kHz sampling (ok for resonator pitches up to 2.5 kHz)
													// using a lower sampling rate for a resonator increases speed, but
													// resonators based on different sampling rates will exhibit slight
													// discrepancies in their power levels due to signal filtering.

													// Application Note:
													// the lo and hi notes on an 88-key piano are 27.5 Hz and 4186 Hz,
													// so for a smooth-response resonator bank covering the piano use:
													// resonatorForm = 88 resonators
													// pitch = 27.5 Hz (lo A, four octaves below A440)
													// width = 1.0 semitone
													// fixedsampling = true
													// decimation = 2 (5 kHz covers hi C at 4186 Hz)
} ResonatorInfo;

//-----------------------------------------------------------------------------

typedef union		{struct {unsigned long mag : 10; long dir : 22;}; long n;} Vector;

// Callback function provided by the Plugin for Volve-Map generation
typedef void	(*PluginMapFunc)(double *xcoord, double *ycoord);

// opMakeMap and opVolve control information:
// See Notes document for discussion.
// 
typedef struct {
	Raster				*dst;						// pointer to buffer to apply volve
	PluginMapFunc		mapFunc;					// pointer to Mapping function
	long				alpha;						// alpha value: 0 <= alpha <= 255
	long				decay;						// decay value: 0 <= decay <= 255
	long				res;						// supersampling factor: 0 <= res <= 4
	unsigned long		mapSize;					// set by MakeMap, size of map in bytes
	Vector*				map;						// set by MakeMap, locked to size at start of build
	Raster				tmp;						// set by MakeMap, <x0,y0> <x1,y1> modifiable by Plugin
	void*				mapState;					// set by MakeMap
} VolveInfo;

// image resource info:
// 1.0b images are local resources in native format
// future releases will also handle PNG 32-bit RGBA format
//
typedef struct {
	char*				name;						// image name
	PluginInfo*			info;						// PluginInfo, info pointer used to find library
} RsrcInfo;

// opComposite control information:
// Source image pixels are transformed and alpha-blended into destination pixels.
// The coordinate system is x positive to the right, y positive down. The floating
// point values offsetx and offsety allow for micropixel motion of images. Images
// are antialiased using supersampling, the quality of which is determined by the
// .res value: supersampling factor = 1 << (2 * .res) samples per destination pixel.
// If src.alphaEnable != 0, src pixels also use their alpha channel for transparency. 
// If dst.alphaEnable != 0, dst pixels also use their alpha channel for permeability.
// See Notes document for discussion.
// 
typedef struct {
	Raster				src;						// source image
	Raster				dst;						// destination image
	// transform applied to src in dst space:
	long				alpha;						// blend value: 0 <= alpha <= 255
	long				res;						// supersampling factor: 0 <= res
	double				scalex, scaley;				// scaling
	double				shearx, sheary;				// shearing
	double				rotate;						// rotation: in degrees clockwise
	double				offsetx, offsety;			// translation: [0,0] = center of dest bounds
} BlitInfo;

enum {
	makeWholeMap	= 1L << 31					// add this to VolveInfo.res to build whole map on first call to opMakeMap
};

enum {boff, goff, roff, aoff};

// The sample rate and number of samples provided in the sound data
#define	RATE				44100L	// 44.1 kHz sample rate

#define	SEMIS(n)			pow(2,(n)/12.)

// opMakeMap is called once to initialize and then xSize * ySize times to complete a Map,
// the very next call will reinitialize and start over.
enum {
	mapMaking = -1,									// returned while Map still has unmapped pixels
	mapError = 0,									// returned on error
	mapDone = +1									// returned when all pixels have been mapped
};


#endif //QCDMODVISUALS_H