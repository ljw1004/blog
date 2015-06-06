#ifndef __luwmp_H
#define __luwmp_H


// Here I have copied out the windows media player 9 SDK header file.
// All this is the property of Microsoft. I hope they don't mind me copying it out!


typedef interface IWMPPlaylist IWMPPlaylist;
typedef interface IWMPErrorItem IWMPErrorItem;
typedef interface IWMPError IWMPError;
typedef interface IWMPMedia IWMPMedia;
typedef interface IWMPMedia2 IWMPMedia2;
typedef interface IWMPMedia3 IWMPMedia3;
typedef interface IWMPControls IWMPControls;
typedef interface IWMPCore IWMPCore;
typedef interface IWMPCore2 IWMPCore2;
typedef interface IWMPCore3 IWMPCore3;
typedef interface IWMPEffects IWMPEffects;
typedef interface IWMPEffects2 IWMPEffects2;
typedef interface IWMPSettings IWMPSettings;
typedef interface IWMPMediaCollection IWMPMediaCollection;
typedef interface IWMPPlaylistCollection IWMPPlaylistCollection;
typedef interface IWMPNetwork IWMPNetwork;
typedef interface IWMPCdromCollection IWMPCdromCollection;
typedef interface IWMPCdrom IWMPCdrom;
typedef interface IWMPClosedCaption IWMPClosedCaption;
typedef interface IWMPDVD IWMPDVD;
typedef interface IWMPStringCollection IWMPStringCollection;
typedef interface IWMPPlaylistArray IWMPPlaylistArray;
typedef interface IWMPMetadataPicture IWMPMetadataPicture;



#define	EFFECT_CANGOFULLSCREEN	( 0x1 )
#define	EFFECT_HASPROPERTYPAGE	( 0x2 )
#define	EFFECT_VARIABLEFREQSTEP	( 0x4 )
#define	SA_BUFFER_SIZE	( 1024 )


enum PlayerState {stop_state=0, pause_state=1, play_state=2};
const float kfltTimedLevelMaximumFrequency = 22050.0F;
const float kfltTimedLevelMinimumFrequency = 20.0F;
inline int FREQUENCY_INDEX(const float FREQ) {return (int)((FREQ-kfltTimedLevelMinimumFrequency) / (kfltTimedLevelMaximumFrequency-kfltTimedLevelMinimumFrequency) * SA_BUFFER_SIZE);}


typedef struct tagTimedLevel
{ unsigned char frequency[2][1024];
  unsigned char waveform[2][1024];
  int state;
  hyper timeStamp;
} TimedLevel;


typedef enum WMPOpenState {
  wmposUndefined = 0,
	wmposPlaylistChanging = wmposUndefined + 1,
	wmposPlaylistLocating	= wmposPlaylistChanging + 1,
	wmposPlaylistConnecting	= wmposPlaylistLocating + 1,
	wmposPlaylistLoading	= wmposPlaylistConnecting + 1,
	wmposPlaylistOpening	= wmposPlaylistLoading + 1,
	wmposPlaylistOpenNoMedia	= wmposPlaylistOpening + 1,
	wmposPlaylistChanged	= wmposPlaylistOpenNoMedia + 1,
	wmposMediaChanging	= wmposPlaylistChanged + 1,
	wmposMediaLocating	= wmposMediaChanging + 1,
	wmposMediaConnecting	= wmposMediaLocating + 1,
	wmposMediaLoading	= wmposMediaConnecting + 1,
	wmposMediaOpening	= wmposMediaLoading + 1,
	wmposMediaOpen	= wmposMediaOpening + 1,
	wmposBeginCodecAcquisition	= wmposMediaOpen + 1,
	wmposEndCodecAcquisition	= wmposBeginCodecAcquisition + 1,
	wmposBeginLicenseAcquisition	= wmposEndCodecAcquisition + 1,
	wmposEndLicenseAcquisition	= wmposBeginLicenseAcquisition + 1,
	wmposBeginIndividualization	= wmposEndLicenseAcquisition + 1,
	wmposEndIndividualization	= wmposBeginIndividualization + 1,
	wmposMediaWaiting	= wmposEndIndividualization + 1,
	wmposOpeningUnknownURL	= wmposMediaWaiting + 1
} WMPOpenState;


typedef enum WMPPlayState {
  wmppsUndefined	= 0,
	wmppsStopped	= wmppsUndefined + 1,
	wmppsPaused	= wmppsStopped + 1,
	wmppsPlaying	= wmppsPaused + 1,
	wmppsScanForward	= wmppsPlaying + 1,
	wmppsScanReverse	= wmppsScanForward + 1,
	wmppsBuffering	= wmppsScanReverse + 1,
	wmppsWaiting	= wmppsBuffering + 1,
	wmppsMediaEnded	= wmppsWaiting + 1,
	wmppsTransitioning	= wmppsMediaEnded + 1,
	wmppsReady	= wmppsTransitioning + 1,
	wmppsReconnecting	= wmppsReady + 1,
	wmppsLast	= wmppsReconnecting + 1
} WMPPlayState;



const IID IID_IWMPStringCollection = {0x4a976298,0x8c0d,0x11d3,{0xb3,0x89,0x00,0xc0,0x4f,0x68,0x57,0x4b}};
MIDL_INTERFACE("4a976298-8c0d-11d3-b389-00c04f68574b")
IWMPStringCollection : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE get_count(long *plCount) = 0;
  virtual HRESULT STDMETHODCALLTYPE item(long lIndex, BSTR *pbstrString) = 0;
};



const IID IID_IWMPErrorItem = {0x3614c646,0x3b3b,0x4de7,{0xa8,0x1e,0x93,0x0e,0x3f,0x21,0x27,0xb3}};
MIDL_INTERFACE("3614C646-3B3B-4de7-A81E-930E3F2127B3")
IWMPErrorItem : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE get_errorCode(long *phr) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_errorDescription(BSTR *pbstrDescription) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_errorContext(VARIANT *pvarContext) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_remedy(long *plRemedy) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_customUrl(BSTR *pbstrCustomUrl) = 0;
};


const IID IID_IWMPError = { 0xA12DCF7D,0x14AB,0x4c1b,{0xA8,0xCD,0x63,0x90,0x9F,0x06,0x02,0x5B}};
MIDL_INTERFACE("A12DCF7D-14AB-4c1b-A8CD-63909F06025B")
IWMPError : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE clearErrorQueue() = 0;
  virtual HRESULT STDMETHODCALLTYPE get_errorCount(long *plNumErrors) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_item(long dwIndex, IWMPErrorItem **ppErrorItem) = 0;
  virtual HRESULT STDMETHODCALLTYPE webHelp() = 0;
};



const IID IID_IWMPMedia = {0x94d55e95, 0x3fac, 0x11d3, {0xb1, 0x55, 0x00, 0xc0, 0x4f, 0x79, 0xfa, 0xa6}};
MIDL_INTERFACE("94D55E95-3FAC-11d3-B155-00C04F79FAA6")
IWMPMedia : public IDispatch
{
public:
  virtual HRESULT STDMETHODCALLTYPE get_isIdentical(IWMPMedia *pIWMPMedia, VARIANT_BOOL *pvbool) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_sourceURL(BSTR *pbstrSourceURL) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_name(BSTR *pbstrName) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_name(BSTR bstrName) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_imageSourceWidth(long *pWidth) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_imageSourceHeight(long *pHeight) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_markerCount(long *pMarkerCount) = 0;
  virtual HRESULT STDMETHODCALLTYPE getMarkerTime(long MarkerNum, double *pMarkerTime) = 0;
  virtual HRESULT STDMETHODCALLTYPE getMarkerName(long MarkerNum, BSTR *pbstrMarkerName) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_duration(double *pDuration) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_durationString(BSTR *pbstrDuration) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_attributeCount(long *plCount) = 0;
  virtual HRESULT STDMETHODCALLTYPE getAttributeName(long lIndex, BSTR *pbstrItemName) = 0;
  virtual HRESULT STDMETHODCALLTYPE getItemInfo(BSTR bstrItemName, BSTR *pbstrVal) = 0;
  virtual HRESULT STDMETHODCALLTYPE setItemInfo(BSTR bstrItemName, BSTR bstrVal) = 0;
  virtual HRESULT STDMETHODCALLTYPE getItemInfoByAtom(long lAtom, BSTR *pbstrVal) = 0;
  virtual HRESULT STDMETHODCALLTYPE isMemberOf(IWMPPlaylist *pPlaylist, VARIANT_BOOL *pvarfIsMemberOf) = 0;
  virtual HRESULT STDMETHODCALLTYPE isReadOnlyItem(BSTR bstrItemName, VARIANT_BOOL *pvarfIsReadOnly) = 0;
};
//    
const IID IID_IWMPMedia2 = {0xab7c88bb, 0x143e, 0x4ea4, {0xac, 0xc3, 0xe4, 0x35, 0x0b, 0x21, 0x06, 0xc3}};
MIDL_INTERFACE("AB7C88BB-143E-4ea4-ACC3-E4350B2106C3")
IWMPMedia2 : public IWMPMedia
{ public:
  virtual HRESULT STDMETHODCALLTYPE get_error(IWMPErrorItem **ppIWMPErrorItem) = 0;
};
//
const IID IID_IWMPMedia3 = {0xf118efc7, 0xf03a, 0x4fb4, {0x99, 0xc9, 0x1c, 0x02, 0xa5, 0xc1, 0x06, 0x5b}};
MIDL_INTERFACE("F118EFC7-F03A-4fb4-99C9-1C02A5C1065B")
IWMPMedia3 : public IWMPMedia2
{ public:
  virtual HRESULT STDMETHODCALLTYPE getAttributeCountByType(BSTR bstrType, BSTR bstrLanguage, long *plCount) = 0;
  virtual HRESULT STDMETHODCALLTYPE getItemInfoByType(BSTR bstrType, BSTR bstrLanguage, long lIndex, VARIANT *pvarValue) = 0;
};



const IID IID_IWMPPlaylist = {0xd5f0f4f1,0x130c,0x11d3,{0xB1,0x4E,0x00,0xC0,0x4F,0x79,0xfa,0xa6}};
MIDL_INTERFACE("D5F0F4F1-130C-11d3-B14E-00C04F79FAA6")
IWMPPlaylist : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE get_count(long *plCount) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_name(BSTR *pbstrName) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_name(BSTR bstrName) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_attributeCount(long *plCount) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_attributeName(long lIndex, BSTR *pbstrAttributeName) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_item(long lIndex, IWMPMedia **ppIWMPMedia) = 0;
  virtual HRESULT STDMETHODCALLTYPE getItemInfo(BSTR bstrName, BSTR *pbstrVal) = 0;
  virtual HRESULT STDMETHODCALLTYPE setItemInfo(BSTR bstrName, BSTR bstrValue) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_isIdentical(IWMPPlaylist *pIWMPPlaylist, VARIANT_BOOL *pvbool) = 0;
  virtual HRESULT STDMETHODCALLTYPE clear() = 0;
  virtual HRESULT STDMETHODCALLTYPE insertItem(long lIndex, IWMPMedia *pIWMPMedia) = 0;
  virtual HRESULT STDMETHODCALLTYPE appendItem(IWMPMedia *pIWMPMedia) = 0;
  virtual HRESULT STDMETHODCALLTYPE removeItem(IWMPMedia *pIWMPMedia) = 0;
  virtual HRESULT STDMETHODCALLTYPE moveItem(long lIndexOld, long lIndexNew) = 0;
};


const IID IID_IWMPPlaylistArray = {0x679409c0,0x99f7,0x11d3,{0x9f,0xb7,0x00,0x10,0x5a,0xa6,0x20,0xbb}};
MIDL_INTERFACE("679409c0-99f7-11d3-9fb7-00105aa620bb")
IWMPPlaylistArray : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE get_count(long *plCount) = 0;
  virtual HRESULT STDMETHODCALLTYPE item(long lIndex, IWMPPlaylist **ppItem) = 0;
};


const IID IID_IWMPMediaCollection = {0x8363BC22,0xB4B4,0x4b19,{0x98,0x9D,0x1C,0xD7,0x65,0x74,0x9D,0xD1}};
MIDL_INTERFACE("8363BC22-B4B4-4b19-989D-1CD765749DD1")
IWMPMediaCollection : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE add(BSTR bstrURL, IWMPMedia **ppItem) = 0;
  virtual HRESULT STDMETHODCALLTYPE getAll(IWMPPlaylist **ppMediaItems) = 0;
  virtual HRESULT STDMETHODCALLTYPE getByName(BSTR bstrName, IWMPPlaylist **ppMediaItems) = 0;
  virtual HRESULT STDMETHODCALLTYPE getByGenre(BSTR bstrGenre, IWMPPlaylist **ppMediaItems) = 0;
  virtual HRESULT STDMETHODCALLTYPE getByAuthor(BSTR bstrAuthor, IWMPPlaylist **ppMediaItems) = 0;
  virtual HRESULT STDMETHODCALLTYPE getByAlbum(BSTR bstrAlbum, IWMPPlaylist **ppMediaItems) = 0;
  virtual HRESULT STDMETHODCALLTYPE getByAttribute(BSTR bstrAttribute, BSTR bstrValue, IWMPPlaylist **ppMediaItems) = 0;
  virtual HRESULT STDMETHODCALLTYPE remove(IWMPMedia *pItem, VARIANT_BOOL varfDeleteFile) = 0;
  virtual HRESULT STDMETHODCALLTYPE getAttributeStringCollection(BSTR bstrAttribute, BSTR bstrMediaType, IWMPStringCollection **ppStringCollection) = 0;
  virtual HRESULT STDMETHODCALLTYPE getMediaAtom(BSTR bstrItemName, long *plAtom) = 0;
  virtual HRESULT STDMETHODCALLTYPE setDeleted(IWMPMedia *pItem, VARIANT_BOOL varfIsDeleted) = 0;
  virtual HRESULT STDMETHODCALLTYPE isDeleted(IWMPMedia *pItem, VARIANT_BOOL *pvarfIsDeleted) = 0;
};


const IID IID_IWMPPlaylistCollection = {0x10A13217,0x23A7,0x439b,{0xB1,0xC0,0xD8,0x47,0xC7,0x9B,0x77,0x74}};
MIDL_INTERFACE("10A13217-23A7-439b-B1C0-D847C79B7774")
IWMPPlaylistCollection : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE newPlaylist(BSTR bstrName, IWMPPlaylist **ppItem) = 0;
  virtual HRESULT STDMETHODCALLTYPE getAll(IWMPPlaylistArray **ppPlaylistArray) = 0;
  virtual HRESULT STDMETHODCALLTYPE getByName(BSTR bstrName, IWMPPlaylistArray **ppPlaylistArray) = 0;
  virtual HRESULT STDMETHODCALLTYPE remove(IWMPPlaylist *pItem) = 0;
  virtual HRESULT STDMETHODCALLTYPE setDeleted(IWMPPlaylist *pItem, VARIANT_BOOL varfIsDeleted) = 0;
  virtual HRESULT STDMETHODCALLTYPE isDeleted(IWMPPlaylist *pItem, VARIANT_BOOL *pvarfIsDeleted) = 0;
  virtual HRESULT STDMETHODCALLTYPE importPlaylist(IWMPPlaylist *pItem, IWMPPlaylist **ppImportedItem) = 0;
};


const IID IID_IWMPControls = {0x74c09e02, 0xf828, 0x11d2, {0xa7, 0x4b, 0x00, 0xa0, 0xc9, 0x05, 0xf3, 0x6e}};
MIDL_INTERFACE("74C09E02-F828-11d2-A74B-00A0C905F36E")
IWMPControls : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE get_isAvailable(BSTR bstrItem, VARIANT_BOOL *pIsAvailable) = 0;
  virtual HRESULT STDMETHODCALLTYPE play() = 0;
  virtual HRESULT STDMETHODCALLTYPE stop() = 0;
  virtual HRESULT STDMETHODCALLTYPE pause() = 0;
  virtual HRESULT STDMETHODCALLTYPE fastForward() = 0;
  virtual HRESULT STDMETHODCALLTYPE fastReverse() = 0;
  virtual HRESULT STDMETHODCALLTYPE get_currentPosition(double *pdCurrentPosition) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_currentPosition(double dCurrentPosition) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_currentPositionString(BSTR *pbstrCurrentPosition) = 0;
  virtual HRESULT STDMETHODCALLTYPE next() = 0;
  virtual HRESULT STDMETHODCALLTYPE previous() = 0;
  virtual HRESULT STDMETHODCALLTYPE get_currentItem(IWMPMedia **ppIWMPMedia) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_currentItem(IWMPMedia *pIWMPMedia) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_currentMarker(long *plMarker) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_currentMarker(long lMarker) = 0;
  virtual HRESULT STDMETHODCALLTYPE playItem(IWMPMedia *pIWMPMedia) = 0;
};


const IID IID_IWMPCore = {0xd84cca99, 0xcce2, 0x11d2, {0x9e, 0xcc, 0x00, 0x00, 0xf8, 0x08, 0x59, 0x81}};
MIDL_INTERFACE("D84CCA99-CCE2-11d2-9ECC-0000F8085981")
IWMPCore : public IDispatch
{
public:
  virtual HRESULT STDMETHODCALLTYPE close() = 0;
  virtual HRESULT STDMETHODCALLTYPE get_URL(BSTR *pbstrURL) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_URL(BSTR bstrURL) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_openState(WMPOpenState *pwmpos) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_playState(WMPPlayState *pwmpps) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_controls(IWMPControls **ppControl) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_settings(IWMPSettings **ppSettings) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_currentMedia(IWMPMedia **ppMedia) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_currentMedia(IWMPMedia *pMedia) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_mediaCollection(IWMPMediaCollection **ppMediaCollection) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_playlistCollection(IWMPPlaylistCollection **ppPlaylistCollection) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_versionInfo(BSTR *pbstrVersionInfo) = 0;
  virtual HRESULT STDMETHODCALLTYPE launchURL(BSTR bstrURL) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_network(IWMPNetwork **ppQNI) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_currentPlaylist(IWMPPlaylist **ppPL) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_currentPlaylist(IWMPPlaylist *pPL) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_cdromCollection(IWMPCdromCollection **ppCdromCollection) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_closedCaption(IWMPClosedCaption **ppClosedCaption) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_isOnline(VARIANT_BOOL *pfOnline) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_error(IWMPError **ppError) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_status(BSTR *pbstrStatus) = 0;
};
//
const IID IID_IWMPCore2 = {0xbc17e5b7, 0x7561, 0x4c18, {0xbb, 0x90, 0x17, 0xd4, 0x85, 0x77, 0x56, 0x59}};
MIDL_INTERFACE("BC17E5B7-7561-4c18-BB90-17D485775659")
IWMPCore2 : public IWMPCore
{
public:
  virtual HRESULT STDMETHODCALLTYPE get_dvd(IWMPDVD **ppDVD) = 0;
};
//
const IID IID_IWMPCore3 = {0x7587c667, 0x628f, 0x499f, {0x88, 0xe7, 0x6a, 0x6f, 0xe8, 0x88, 0x84, 0x64}};
MIDL_INTERFACE("7587C667-628F-499f-88E7-6A6F4E888464")
IWMPCore3 : public IWMPCore2
{
public:
  virtual HRESULT STDMETHODCALLTYPE newPlaylist(BSTR bstrName,BSTR bstrURL,IWMPPlaylist **ppPlaylist) = 0;
  virtual HRESULT STDMETHODCALLTYPE newMedia(BSTR bstrURL,IWMPMedia **ppMedia) = 0;
};





const IID IID_IWMPSettings = {0x9104D1AB,0x80C9,0x4fed,{0xAB,0xF0,0x2E,0x64,0x17,0xA6,0xDF,0x14}};
MIDL_INTERFACE("9104D1AB-80C9-4fed-ABF0-2E6417A6DF14")
IWMPSettings : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE get_isAvailable(BSTR bstrItem, VARIANT_BOOL *pIsAvailable) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_autoStart(VARIANT_BOOL *pfAutoStart) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_autoStart(VARIANT_BOOL fAutoStart) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_baseURL(BSTR *pbstrBaseURL) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_baseURL(BSTR bstrBaseURL) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_defaultFrame(BSTR *pbstrDefaultFrame) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_defaultFrame(BSTR bstrDefaultFrame) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_invokeURLs(VARIANT_BOOL *pfInvokeURLs) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_invokeURLs(VARIANT_BOOL fInvokeURLs) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_mute(VARIANT_BOOL *pfMute) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_mute(VARIANT_BOOL fMute) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_playCount(long *plCount) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_playCount(long lCount) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_rate(double *pdRate) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_rate(double dRate) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_balance(long *plBalance) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_balance(long lBalance) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_volume(long *plVolume) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_volume(long lVolume) = 0;
  virtual HRESULT STDMETHODCALLTYPE getMode(BSTR bstrMode, VARIANT_BOOL *pvarfMode) = 0;
  virtual HRESULT STDMETHODCALLTYPE setMode(BSTR bstrMode, VARIANT_BOOL varfMode) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_enableErrorDialogs(VARIANT_BOOL *pfEnableErrorDialogs) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_enableErrorDialogs(VARIANT_BOOL fEnableErrorDialogs) = 0;
};


const IID IID_IWMPNetwork = { 0xEC21B779,0xEDEF,0x462d,{0xBB,0xA4,0xAD,0x9D,0xDE,0x2B,0x29,0xA7}};
MIDL_INTERFACE("EC21B779-EDEF-462d-BBA4-AD9DDE2B29A7")
IWMPNetwork : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE get_bandWidth(long *plBandwidth) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_recoveredPackets(long *plRecoveredPackets) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_sourceProtocol(BSTR *pbstrSourceProtocol) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_receivedPackets(long *plReceivedPackets) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_lostPackets(long *plLostPackets) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_receptionQuality(long *plReceptionQuality) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_bufferingCount(long *plBufferingCount) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_bufferingProgress(long *plBufferingProgress) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_bufferingTime(long *plBufferingTime) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_bufferingTime(long lBufferingTime) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_frameRate(long *plFrameRate) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_maxBitRate(long *plBitRate) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_bitRate(long *plBitRate) = 0;
  virtual HRESULT STDMETHODCALLTYPE getProxySettings(BSTR bstrProtocol, long *plProxySetting) = 0;
  virtual HRESULT STDMETHODCALLTYPE setProxySettings(BSTR bstrProtocol, long lProxySetting) = 0;
  virtual HRESULT STDMETHODCALLTYPE getProxyName(BSTR bstrProtocol, BSTR *pbstrProxyName) = 0;
  virtual HRESULT STDMETHODCALLTYPE setProxyName(BSTR bstrProtocol, BSTR bstrProxyName) = 0;
  virtual HRESULT STDMETHODCALLTYPE getProxyPort(BSTR bstrProtocol, long *lProxyPort) = 0;
  virtual HRESULT STDMETHODCALLTYPE setProxyPort(BSTR bstrProtocol, long lProxyPort) = 0;
  virtual HRESULT STDMETHODCALLTYPE getProxyExceptionList(BSTR bstrProtocol, BSTR *pbstrExceptionList) = 0;
  virtual HRESULT STDMETHODCALLTYPE setProxyExceptionList(BSTR bstrProtocol, BSTR pbstrExceptionList) = 0;
  virtual HRESULT STDMETHODCALLTYPE getProxyBypassForLocal(BSTR bstrProtocol, VARIANT_BOOL *pfBypassForLocal) = 0;
  virtual HRESULT STDMETHODCALLTYPE setProxyBypassForLocal(BSTR bstrProtocol, VARIANT_BOOL fBypassForLocal) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_maxBandwidth(long *lMaxBandwidth) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_maxBandwidth(long lMaxBandwidth) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_downloadProgress(long *plDownloadProgress) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_encodedFrameRate(long *plFrameRate) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_framesSkipped(long *plFrames) = 0;
};



const IID IID_IWMPCdrom = { 0xcfab6e98,0x8730,0x11d3,{0xb3,0x88,0x00,0xc0,0x4f,0x68,0x57,0x4b}};
MIDL_INTERFACE("cfab6e98-8730-11d3-b388-00c04f68574b")
IWMPCdrom : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE get_driveSpecifier(BSTR *pbstrDrive) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_playlist(IWMPPlaylist **ppPlaylist) = 0;
  virtual HRESULT STDMETHODCALLTYPE eject() = 0;
};


const IID IID_IWMPCdromCollection = { 0xEE4C8FE2,0x34B2,0x11d3,{0xA3,0xBF,0x00,0x60,0x97,0xC9,0xB3,0x44}};
MIDL_INTERFACE("EE4C8FE2-34B2-11d3-A3BF-006097C9B344")
IWMPCdromCollection : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE get_count(long *plCount) = 0;
  virtual HRESULT STDMETHODCALLTYPE item(long lIndex, IWMPCdrom **ppItem) = 0;
  virtual HRESULT STDMETHODCALLTYPE getByDriveSpecifier(BSTR bstrDriveSpecifier, IWMPCdrom **ppCdrom) = 0;
};

const IID IID_IWMPDVD = {0x8DA61686,0x4668,0x4a5c,{0xAE,0x5D,0x80,0x31,0x93,0x29,0x3D,0xBE}};
MIDL_INTERFACE("8DA61686-4668-4a5c-AE5D-803193293DBE")
IWMPDVD : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE get_isAvailable(BSTR bstrItem, VARIANT_BOOL *pIsAvailable) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_domain(BSTR *strDomain) = 0;
  virtual HRESULT STDMETHODCALLTYPE topMenu() = 0;
  virtual HRESULT STDMETHODCALLTYPE titleMenu() = 0;
  virtual HRESULT STDMETHODCALLTYPE back() = 0;
  virtual HRESULT STDMETHODCALLTYPE resume() = 0;
};



const IID IID_IWMPClosedCaption = {0x4F2DF574,0xC588,0x11d3,{0x9E,0xD0,0x00,0xC0,0x4F,0xB6,0xE9,0x37}};
MIDL_INTERFACE("4F2DF574-C588-11d3-9ED0-00C04FB6E937")
IWMPClosedCaption : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE get_SAMIStyle(BSTR *pbstrSAMIStyle) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_SAMIStyle(BSTR bstrSAMIStyle) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_SAMILang(BSTR *pbstrSAMILang) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_SAMILang(BSTR bstrSAMILang) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_SAMIFileName(BSTR *pbstrSAMIFileName) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_SAMIFileName(BSTR bstrSAMIFileName) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_captioningId(BSTR *pbstrCaptioningID) = 0;
  virtual HRESULT STDMETHODCALLTYPE put_captioningId(BSTR bstrCaptioningID) = 0;
};



const IID IID_IWMPMetadataPicture = {0x5C29BBE0,0xF87D,0x4c45,{0xAA,0x28,0xA7,0x0F,0x02,0x30,0xFF,0xA9}};
MIDL_INTERFACE("5C29BBE0-F87D-4c45-AA28-A70F0230FFA9")
IWMPMetadataPicture : public IDispatch
{ public:
  virtual HRESULT STDMETHODCALLTYPE get_mimeType(BSTR *pbstrMimeType) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_pictureType(BSTR *pbstrPictureType) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_description(BSTR *pbstrDescription) = 0;
  virtual HRESULT STDMETHODCALLTYPE get_URL(BSTR *pbstrURL) = 0;
};



const IID IID_IWMPEffects = {0xd3984c13, 0xc3cb, 0x48e2, {0x8b, 0xe5, 0x51, 0x68, 0x34, 0x0b, 0x4f, 0x35}};
MIDL_INTERFACE("D3984C13-C3CB-48e2-8BE5-5168340B4F35")
IWMPEffects : public IUnknown
{
public:
  virtual HRESULT STDMETHODCALLTYPE Render(TimedLevel *pLevels, HDC hdc, RECT *prc) = 0;
  virtual HRESULT STDMETHODCALLTYPE MediaInfo(LONG lChannelCount, LONG lSampleRate, BSTR bstrTitle) = 0;
  virtual HRESULT STDMETHODCALLTYPE GetCapabilities(DWORD *pdwCapabilities) = 0;
  virtual HRESULT STDMETHODCALLTYPE GetTitle(BSTR *bstrTitle) = 0;
  virtual HRESULT STDMETHODCALLTYPE GetPresetTitle(LONG nPreset, BSTR *bstrPresetTitle) = 0;
  virtual HRESULT STDMETHODCALLTYPE GetPresetCount(LONG *pnPresetCount) = 0;
  virtual HRESULT STDMETHODCALLTYPE SetCurrentPreset(LONG nPreset) = 0;
  virtual HRESULT STDMETHODCALLTYPE GetCurrentPreset(LONG *pnPreset) = 0;
  virtual HRESULT STDMETHODCALLTYPE DisplayPropertyPage(HWND hwndOwner) = 0;
  virtual HRESULT STDMETHODCALLTYPE GoFullscreen(BOOL fFullScreen) = 0;
  virtual HRESULT STDMETHODCALLTYPE RenderFullScreen(TimedLevel *pLevels) = 0;
};

const IID IID_IWMPEffects2 = {0x695386ec, 0xaa3c, 0x4618, {0xa5, 0xe1, 0xdd, 0x9a, 0x8b, 0x98, 0x76, 0x32}};
MIDL_INTERFACE("695386EC-AA3C-4618-A5E1-DD9A8B987632")
IWMPEffects2 : public IWMPEffects
{
public:
  virtual HRESULT STDMETHODCALLTYPE SetCore(IWMPCore *pPlayer) = 0;
  virtual HRESULT STDMETHODCALLTYPE Create(HWND hwndParent) = 0;
  virtual HRESULT STDMETHODCALLTYPE Destroy() = 0;
  virtual HRESULT STDMETHODCALLTYPE NotifyNewMedia(IWMPMedia *pMedia) = 0;
  virtual HRESULT STDMETHODCALLTYPE OnWindowMessage(UINT msg,WPARAM WParam,LPARAM LParam,LRESULT *plResultParam) = 0;
  virtual HRESULT STDMETHODCALLTYPE RenderWindowed(TimedLevel *pData,BOOL fRequiredRender) = 0;
};






#endif