// Copyright (c) 1992, 1993, 1994 Henry Spencer.
// Copyright (c) 1992, 1993, 1994 The Regents of the University of California.  All rights reserved.
// Changes in 1998 by Simon Owen, to put it all into a single file and make it compilable in C++
// Changes in 2002 by Lucian Wischik, to change the API a little and remove compiler warnings and changed ISWORD to make it work with hi-ascii characters
// This code is derived from software contributed to Berkeley by Henry Spencer.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by the University of
//    California, Berkeley and its contributors.
// 4. Neither the name of the University nor the names of its contributors
//    may be used to endorse or promote products derived from this software
//    without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS ``AS IS'' AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED.  IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
// OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
// OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
// SUCH DAMAGE.
//


#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <limits.h>
#include <assert.h>
#include "regex.h"

#pragma warning( disable: 4244 )
// the code is not 64bit safe!

#define ASSERT assert



typedef long regoff_t;

typedef struct
{ size_t	    re_nsub;	// number of parenthesized subexpressions
	int	        re_magic;
	const char *re_endp;	// end pointer for REG_PEND
	struct re_guts *re_g;		// none of your business :-)
} regex_it;




#define	REG_PEND	0040
#define REG_NOSUB 0004


// Error return values flags
enum {	REG_SUCCESS, REG_NOMATCH, REG_BADPAT, REG_ECOLLATE, REG_ECTYPE, REG_EESCAPE, REG_ESUBREG, REG_EBRACK,
		REG_EPAREN, REG_EBRACE, REG_BADBR, REG_ERANGE, REG_ESPACE, REG_BADRPT, REG_EMPTY, REG_ASSERT,
		REG_INVARG, REG_ATOI = 255, REG_ITOA = 0400 };

// Compile flags
enum {	REG_NOTBOL = 00001, REG_NOTEOL = 00002, REG_STARTEND = 00004, REG_TRACE = 00400,
		REG_LARGE = 01000, REG_BACKR = 02000 };


void regfree(regex_it *preg);



////////////////////////////////////////////////////////////////////////////////
//	U T I L S . H

// utility definitions
#define	DUPMAX		255
#define	INFINITY	(DUPMAX + 1)
#define	NC		(CHAR_MAX - CHAR_MIN + 1)
typedef unsigned char uch;


////////////////////////////////////////////////////////////////////////////////
//	R E G E X 2 . H

// internals of regex_it

#define	MAGIC1	((('r'^0200)<<8) | 'e')

/*
 * The internal representation is a *strip*, a sequence of
 * operators ending with an endmarker.  (Some terminology etc. is a
 * historical relic of earlier versions which used multiple strips.)
 * Certain oddities in the representation are there to permit running
 * the machinery backwards; in particular, any deviation from sequential
 * flow must be marked at both its source and its destination.  Some
 * fine points:
 *
 * - OPLUS_ and O_PLUS are *inside* the loop they create.
 * - OQUEST_ and O_QUEST are *outside* the bypass they create.
 * - OCH_ and O_CH are *outside* the multi-way branch they create, while
 *   OOR1 and OOR2 are respectively the end and the beginning of one of
 *   the branches.  Note that there is an implicit OOR2 following OCH_
 *   and an implicit OOR1 preceding O_CH.
 *
 * In state representations, an operator's bit is on to signify a state
 * immediately *preceding* "execution" of that operator.
 */
typedef unsigned long sop;	// strip operator 

typedef long sopno;
#define	OPRMASK	0xf8000000
#define	OPDMASK	0x07ffffff
#define	OPSHIFT	((unsigned)27)
#define	OP(n)	((sopno)((n)&OPRMASK))
#define	OPND(n)	((sopno)((n)&OPDMASK))
#define	SOP(op, opnd)	((op)|(opnd))
// operators			   meaning	operand
//						(back, fwd are offsets)
#define	OEND	(1<<OPSHIFT)	// endmarker
#define	OCHAR	(2<<OPSHIFT)	// character	unsigned char
#define	OBOL	(3<<OPSHIFT)	// left anchor
#define	OEOL	(4<<OPSHIFT)	// right anchor
#define	OANY	(5<<OPSHIFT)	// .
#define	OANYOF	(6<<OPSHIFT)	// [...]	set number
#define	OBACK_	(7<<OPSHIFT)	// begin \d	paren number
#define	O_BACK	(8<<OPSHIFT)	// end \d	paren number
#define	OPLUS_	(9<<OPSHIFT)	// + prefix	fwd to suffix
#define	O_PLUS	(10<<OPSHIFT)	// + suffix	back to prefix
#define	OQUEST_	(11<<OPSHIFT)	// ? prefix	fwd to suffix
#define	O_QUEST	(12<<OPSHIFT)	// ? suffix	back to prefix
#define	OLPAREN	(13<<OPSHIFT)	// (		fwd to )
#define	ORPAREN	(14<<OPSHIFT)	// )		back to (
#define	OCH_	(15<<OPSHIFT)	// begin choice	fwd to OOR2
#define	OOR1	(16<<OPSHIFT)	// | pt. 1	back to OOR1 or OCH_
#define	OOR2	(17<<OPSHIFT)	// | pt. 2	fwd to OOR2 or O_CH
#define	O_CH	(18<<OPSHIFT)	// end choice	back to OOR1
#define	OBOW	(19<<OPSHIFT)	// begin word
#define	OEOW	(20<<OPSHIFT)	// end woRD

/*
 * Structure for [] character-set representation.  Character sets are
 * done as bit vectors, grouped 8 to a byte vector for compactness.
 * The individual set therefore has both a pointer to the byte vector
 * and a mask to pick out the relevant bit of each byte.  A hash code
 * simplifies testing whether two sets could be identical.
 *
 * This will get trickier for multicharacter collating elements.  As
 * preliminary hooks for dealing with such things, we also carry along
 * a string of multi-character elements, and decide the size of the
 * vectors at run time.
 */
typedef struct {
	uch *ptr;		// -> uch [csetsize] 
	uch mask;		// bit within array 
	uch hash;		// hash code 
	size_t smultis;
	char *multis;		// -> char[smulti]  ab\0cd\0ef\0\0 
} cset;

// note that CHadd and CHsub are unsafe, and CHIN doesn't yield 0/1 

#define	CHadd(cs, c)	((cs)->ptr[(uch)(c)] |= (cs)->mask, (cs)->hash += (uch)(c))
#define	CHsub(cs, c)	((cs)->ptr[(uch)(c)] &= (uch)~(cs)->mask, (cs)->hash -= (uch)(c))
#define	CHIN(cs, c)	((cs)->ptr[(uch)(c)] & (cs)->mask)
#define	MCadd(p, cs, cp)	mcadd(p, cs, cp)	// regcomp() internal fns 

#define	MCsub(p, cs, cp)	mcsub(p, cs, cp)
#define	MCin(p, cs, cp)	mcin(p, cs, cp)

// stuff for character categories 

typedef unsigned char cat_t;

/*
 * main compiled-expression structure
 */
struct re_guts {
	int magic;
#define	MAGIC2	((('R'^0200)<<8)|'E')
	sop *strip;		// malloced area for strip 
	int csetsize;		// number of bits in a cset vector 
	int ncsets;		// number of csets in use 
	cset *sets;		// -> cset [ncsets] 
	uch *setbits;		// -> uch[csetsize][ncsets/CHAR_BIT] 
	int cflags;		// copy of regcomp() cflags argument 
	sopno nstates;		// = number of sops 
	sopno firststate;	// the initial OEND (normally 0) 
	sopno laststate;	// the final OEND 
	int iflags;		// internal flags 
#define	USEBOL	01	// used ^ 
#define	USEEOL	02	// used $ 
#define	BAD	04	// something wrong 
	int nbol;		// number of ^ used 
	int neol;		// number of $ used 
	int ncategories;	// how many character categories 
	cat_t *categories;	// ->catspace[-CHAR_MIN] 
	char *must;		// match must contain this string 
	int mlen;		// length of must 
	size_t nsub;		// copy of re_nsub 
	int backrefs;		// does it use back references? 
	sopno nplus;		// how deep does it nest +s? 
	// catspace must be last 
	cat_t catspace[1];	// actually [NC] 
};

// misc utilities 

#define	REGOUT	(CHAR_MAX+1)	// a non-character value 

#define	ISWORD(c)	(isalnum((unsigned char)c) || (c) == '_')


////////////////////////////////////////////////////////////////////////////////
//	C C L A S S . H

// Character-class table
static struct cclass
{
	char *name;
	char *chars;
	char *multis;
}
cclasses[] =
{
	{"alnum",	"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", ""},
	{"alpha",	"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", ""},
	{"blank",	" \t", ""},
	{"cntrl",	"\007\b\t\n\v\f\r\1\2\3\4\5\6\16\17\20\21\22\23\24\25\26\27\30\31\32\33\34\35\36\37\177", ""},
	{"digit",	"0123456789",	""},
	{"graph",	"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~", ""},
	{"lower",	"abcdefghijklmnopqrstuvwxyz", ""},
	{"print",	"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~ ", ""},
	{"punct",	"!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~", ""},
	{"space",	"\t\n\v\f\r ",	""},
	{"upper",	"ABCDEFGHIJKLMNOPQRSTUVWXYZ", ""},
	{"xdigit",	"0123456789ABCDEFabcdef", ""},
	{NULL,		0,		""}
};

////////////////////////////////////////////////////////////////////////////////
//	C N A M E . H

// character-name table
static struct cname
{
	char *name;
	char code;
}
cnames[] =
{
	{"NUL",					'\0'},
	{"SOH",					'\001'},
	{"STX",					'\002'},
	{"ETX",					'\003'},
	{"EOT",					'\004'},
	{"ENQ",					'\005'},
	{"ACK",					'\006'},
	{"BEL",					'\007'},
	{"alert",				'\007'},
	{"BS",					'\010'},
	{"backspace",			'\b'},
	{"HT",					'\011'},
	{"tab",					'\t'},
	{"LF",					'\012'},
	{"newline",				'\n'},
	{"VT",					'\013'},
	{"vertical-tab",			'\v'},
	{"FF",					'\014'},
	{"form-feed",			'\f'},
	{"CR",					'\015'},
	{"carriage-return",		'\r'},
	{"SO",					'\016'},
	{"SI",					'\017'},
	{"DLE",					'\020'},
	{"DC1",					'\021'},
	{"DC2",					'\022'},
	{"DC3",					'\023'},
	{"DC4",					'\024'},
	{"NAK",					'\025'},
	{"SYN",					'\026'},
	{"ETB",					'\027'},
	{"CAN",					'\030'},
	{"EM",					'\031'},
	{"SUB",					'\032'},
	{"ESC",					'\033'},
	{"IS4",					'\034'},
	{"FS",					'\034'},
	{"IS3",					'\035'},
	{"GS",					'\035'},
	{"IS2",					'\036'},
	{"RS",					'\036'},
	{"IS1",					'\037'},
	{"US",					'\037'},
	{"space",				' '},
	{"exclamation-mark",		'!'},
	{"quotation-mark",		'"'},
	{"number-sign",			'#'},
	{"dollar-sign",			'$'},
	{"percent-sign",			'%'},
	{"ampersand",			'&'},
	{"apostrophe",			'\''},
	{"left-parenthesis",		'('},
	{"right-parenthesis",	')'},
	{"asterisk",				'*'},
	{"plus-sign",			'+'},
	{"comma",				','},
	{"hyphen",				'-'},
	{"hyphen-minus",			'-'},
	{"period",				'.'},
	{"full-stop",			'.'},
	{"slash",				'/'},
	{"solidus",				'/'},
	{"zero",					'0'},
	{"one",					'1'},
	{"two",					'2'},
	{"three",				'3'},
	{"four",					'4'},
	{"five",					'5'},
	{"six",					'6'},
	{"seven",				'7'},
	{"eight",				'8'},
	{"nine",					'9'},
	{"colon",				':'},
	{"semicolon",			';'},
	{"less-than-sign",		'<'},
	{"equals-sign",			'='},
	{"greater-than-sign",	'>'},
	{"question-mark",		'?'},
	{"commercial-at",		'@'},
	{"left-square-bracket",	'['},
	{"backslash",			'\\'},
	{"reverse-solidus",		'\\'},
	{"right-square-bracket",	']'},
	{"circumflex",			'^'},
	{"circumflex-accent",	'^'},
	{"underscore",			'_'},
	{"low-line",				'_'},
	{"grave-accent",			'`'},
	{"left-brace",			'{'},
	{"left-curly-bracket",	'{'},
	{"vertical-line",		'|'},
	{"right-brace",			'}'},
	{"right-curly-bracket",	'}'},
	{"tilde",				'~'},
	{"DEL",					'\177'},
	{NULL,	0}
};


////////////////////////////////////////////////////////////////////////////////
//	R E G E X E C . C

/*
 * the outer shell of regexec()
 *
 * This file includes engine.c *twice*, after muchos fiddling with the
 * macros that code uses.  This lets the same code operate on two different
 * representations for state sets.
 */

// macros for manipulating states, small version 

#define	states	long
#define	states1	states		// for later use in regexec() decision 

#define	CLEAR(v)	((v) = 0)
#define	SET0(v, n)	((v) &= ~(1 << (n)))
#define	SET1(v, n)	((v) |= 1 << (n))
#define	ISSET(v, n)	((v) & (1 << (n)))
#define	ASSIGN(d, s)	((d) = (s))
#define	EQ(a, b)	((a) == (b))
#define	STATEVARS	int dummy	// dummy version 

#define	STATESETUP(m, n)	// nothing 

#define	STATETEARDOWN(m)	// nothing 

#define	SETUP(v)	((v) = 0)
#define	onestate	int
#define	INIT(o, n)	((o) = (unsigned)1 << (n))
#define	INC(o)	((o) <<= 1)
#define	ISSTATEIN(v, o)	((v) & (o))
// some abbreviations; note that some of these know variable names! 

// do "if I'm here, I can also be there" etc without branches 

#define	FWD(dst, src, n)	((dst) |= ((unsigned)(src)&(here)) << (n))
#define	BACK(dst, src, n)	((dst) |= ((unsigned)(src)&(here)) >> (n))
#define	ISSETBACK(v, n)	((v) & ((unsigned)here >> (n)))
// function names 



////////////////////////////////////////////////////////////////////////////////
//	E N G I N E . C

#define	matcher	smatcher
#define	fast	sfast
#define	slow	sslow
#define	dissect	sdissect
#define	backref	sbackref
#define	step	sstep
#define	print	sprint
#define	at	sat
#define	match	smat

struct match {
	struct re_guts *g;
	int eflags;
	REGMATCH *pmatch;	// [nsub+1] (0 element unused) 
	char *offp;		// offsets work from here 
	char *beginp;		// start of string -- virtual NUL precedes 
	char *endp;		// end of string -- virtual NUL here 
	char *coldp;		// can be no match starting before here 
	char **lastpos;		// [nplus+1] 
	STATEVARS;
	states st;		// current states 
	states fresh;		// states for a fresh start 
	states tmp;		// temporary 
	states empty;		// empty set of states 
};

// === engine.c === 

static int matcher (struct re_guts *g, char *string, size_t nmatch, REGMATCH pmatch[], int eflags);
static char *dissect (struct match *m, char *start, char *stop, sopno startst, sopno stopst);
static char *backref (struct match *m, char *start, char *stop, sopno startst, sopno stopst, sopno lev);
static char *fast (struct match *m, char *start, char *stop, sopno startst, sopno stopst);
static char *slow (struct match *m, char *start, char *stop, sopno startst, sopno stopst);
static states step (struct re_guts *g, sopno start, sopno stop, states bef, int ch, states aft);
#define	BOL	(REGOUT+1)
#define	EOL	(BOL+1)
#define	BOLEOL	(BOL+2)
#define	NOTHING	(BOL+3)
#define	BOW	(BOL+4)
#define	EOW	(BOL+5)
#define	CODEMAX	(BOL+5)		// highest code used 

#define	NONCHAR(c)	((c) > CHAR_MAX)
#define	NNONCHAR	(CODEMAX-CHAR_MAX)

#define	SP(t, s, c)	// nothing 
#define	AT(t, p1, p2, s1, s2)	// nothing 
#define	NOTE(s)	// nothing 

/*
 - matcher - the actual matching engine
 == static int matcher(register struct re_guts *g, char *string, \
 ==	size_t nmatch, REGMATCH pmatch[], int eflags);
 */
static int			// 0 success, REG_NOMATCH failure 

matcher(register struct re_guts *g, char *string, size_t nmatch, REGMATCH pmatch[], int eflags)
{
	register char *endp;
	register int i;
	struct match mv;
	register struct match *m = &mv;
	register char *dp;
	const register sopno gf = g->firststate+1;	// +1 for OEND 

	const register sopno gl = g->laststate;
	char *start;
	char *stop;

	// simplify the situation where possible 

	if (g->cflags&REG_NOSUB)
		nmatch = 0;
	if (eflags&REG_STARTEND) {
		start = string + pmatch[0].i;
		stop = string + pmatch[0].len;
	} else {
		start = string;
		stop = start + strlen(start);
	}
	if (stop < start)
		return(REG_INVARG);

	// prescreening; this does wonders for this rather slow code 

	if (g->must) {
		for (dp = start; dp < stop; dp++)
			if (*dp == g->must[0] && stop - dp >= g->mlen &&
				memcmp(dp, g->must, (size_t)g->mlen) == 0)
				break;
		if (dp == stop)		// we didn't find g->must 

			return(REG_NOMATCH);
	}

	// match struct setup 

	m->g = g;
	m->eflags = eflags;
	m->pmatch = NULL;
	m->lastpos = NULL;
	m->offp = string;
	m->beginp = start;
	m->endp = stop;
	STATESETUP(m, 4);
	SETUP(m->st);
	SETUP(m->fresh);
	SETUP(m->tmp);
	SETUP(m->empty);
	CLEAR(m->empty);

	// this loop does only one repetition except for backrefs
	for (;;) {
		endp = fast(m, start, stop, gf, gl);
		if (!endp) {		// a miss 

			STATETEARDOWN(m);
			return(REG_NOMATCH);
		}
		if (nmatch == 0 && !g->backrefs)
			break;		// no further info needed 


		// where? 

		ASSERT(m->coldp);
		for (;;) {
			NOTE("finding start");
			endp = slow(m, m->coldp, stop, gf, gl);
			if (endp)
				break;
			ASSERT(m->coldp < m->endp);
			m->coldp++;
		}
		if (nmatch == 1 && !g->backrefs)
			break;		// no further info needed 


		// oh my, he wants the subexpressions... 

		if (!m->pmatch)
			m->pmatch = (REGMATCH *)malloc((m->g->nsub + 1) *
							sizeof(REGMATCH));
		if (!m->pmatch) {
			STATETEARDOWN(m);
			return(REG_ESPACE);
		}
		for (i = 1; i <= (int)(m->g->nsub); i++)
			m->pmatch[i].i = m->pmatch[i].len = -1;
		if (!g->backrefs && !(m->eflags&REG_BACKR)) {
			NOTE("dissecting");
			dp = dissect(m, m->coldp, endp, gf, gl);
		} else {
			if (g->nplus > 0 && !m->lastpos)
				m->lastpos = (char **)malloc((g->nplus+1) *
							sizeof(char *));
			if (g->nplus > 0 && !m->lastpos) {
				free(m->pmatch);
				STATETEARDOWN(m);
				return(REG_ESPACE);
			}
			NOTE("backref dissect");
			dp = backref(m, m->coldp, endp, gf, gl, (sopno)0);
		}
		if (dp)
			break;

		// uh-oh... we couldn't find a subexpression-level match 

		ASSERT(g->backrefs);	// must be back references doing it 

		ASSERT(g->nplus == 0 || m->lastpos);
		for (;;) {
			if (dp || endp <= m->coldp)
				break;		// defeat 

			NOTE("backoff");
			endp = slow(m, m->coldp, endp-1, gf, gl);
			if (!endp)
				break;		// defeat 

			// try it on a shorter possibility 

#ifndef NDEBUG
			for (i = 1; i <= (int)(m->g->nsub); i++) {
				ASSERT(m->pmatch[i].i == -1);
				ASSERT(m->pmatch[i].len == -1);
			}
#endif
			NOTE("backoff dissect");
			dp = backref(m, m->coldp, endp, gf, gl, (sopno)0);
		}
		ASSERT(!dp || dp == endp);
		if (dp)		// found a shorter one 

			break;

		// despite initial appearances, there is no match here 

		NOTE("false alarm");
		start = m->coldp + 1;	// recycle starting later 

		ASSERT(start <= stop);
	}

	// fill in the details if requested 

	if (nmatch > 0) {
		pmatch[0].i = m->coldp - m->offp;
		pmatch[0].len = endp - m->offp;
	}
	if (nmatch > 1) {
		ASSERT(m->pmatch);
		for (i = 1; i < (int)nmatch; i++)
			if (i <= (int)(m->g->nsub))
				pmatch[i] = m->pmatch[i];
			else {
				pmatch[i].i = -1;
				pmatch[i].len = -1;
			}
	}

	if (m->pmatch)
		free((char *)m->pmatch);
	if (m->lastpos)
		free((char *)m->lastpos);
	STATETEARDOWN(m);
	return(0);
}

/*
 - dissect - figure out what matched what, no back references
 == static char *dissect(register struct match *m, char *start, \
 ==	char *stop, sopno startst, sopno stopst);
 */
static char *			// == stop (success) always 

dissect(register struct match *m, char *start, char *stop, sopno startst, sopno stopst)
{
	register int i;
	register sopno ss;	// start sop of current subRE 
	register sopno es;	// end sop of current subRE 
	register char *sp;	// start of string matched by it 
	register char *stp;	// string matched by it cannot pass here 
	register char *rest;	// start of rest of string 
	register char *tail;	// string unmatched by rest of RE 
	register sopno ssub;	// start sop of subsubRE 
	register sopno esub;	// end sop of subsubRE 
	register char *ssp;	// start of string matched by subsubRE 
	register char *sep;	// end of string matched by subsubRE 
	register char *oldssp;	// previous ssp 
	register char *dp;
	AT("diss", start, stop, startst, stopst);
	sp = start;
	for (ss = startst; ss < stopst; ss = es) {
		// identify end of subRE 

		es = ss;
		switch (OP(m->g->strip[es])) {
		case OPLUS_:
		case OQUEST_:
			es += OPND(m->g->strip[es]);
			break;
		case OCH_:
			while (OP(m->g->strip[es]) != O_CH)
				es += OPND(m->g->strip[es]);
			break;
		}
		es++;

		// figure out what it matched 

		switch (OP(m->g->strip[ss])) {
		case OEND:
			ASSERT(false);
			break;
		case OCHAR:
			sp++;
			break;
		case OBOL:
		case OEOL:
		case OBOW:
		case OEOW:
			break;
		case OANY:
		case OANYOF:
			sp++;
			break;
		case OBACK_:
		case O_BACK:
			ASSERT(false);
			break;
		// cases where length of match is hard to find 

		case OQUEST_:
			stp = stop;
			for (;;) {
				// how long could this one be? 

				rest = slow(m, sp, stp, ss, es);
				ASSERT(rest);	// it did match 

				// could the rest match the rest? 

				tail = slow(m, rest, stop, es, stopst);
				if (tail == stop)
					break;		// yes! 

				// no -- try a shorter match for this one 

				stp = rest - 1;
				ASSERT(stp >= sp);	// it did work 

			}
			ssub = ss + 1;
			esub = es - 1;
			// did innards match? 

			if (slow(m, sp, rest, ssub, esub)) {
				dp = dissect(m, sp, rest, ssub, esub);
				ASSERT(dp == rest);
			} else		// no 

				ASSERT(sp == rest);
			sp = rest;
			break;
		case OPLUS_:
			stp = stop;
			for (;;) {
				// how long could this one be? 

				rest = slow(m, sp, stp, ss, es);
				ASSERT(rest);	// it did match 

				// could the rest match the rest? 

				tail = slow(m, rest, stop, es, stopst);
				if (tail == stop)
					break;		// yes! 

				// no -- try a shorter match for this one 

				stp = rest - 1;
				ASSERT(stp >= sp);	// it did work 

			}
			ssub = ss + 1;
			esub = es - 1;
			ssp = sp;
			oldssp = ssp;
			for (;;) {	// find last match of innards 

				sep = slow(m, ssp, rest, ssub, esub);
				if (!sep || sep == ssp)
					break;	// failed or matched null 

				oldssp = ssp;	// on to next try 

				ssp = sep;
			}
			if (!sep) {
				// last successful match 

				sep = ssp;
				ssp = oldssp;
			}
			ASSERT(sep == rest);	// must exhaust substring 

			ASSERT(slow(m, ssp, sep, ssub, esub) == rest);
			dp = dissect(m, ssp, sep, ssub, esub);
			ASSERT(dp == sep);
			sp = rest;
			break;
		case OCH_:
			stp = stop;
			for (;;) {
				// how long could this one be? 

				rest = slow(m, sp, stp, ss, es);
				ASSERT(rest);	// it did match 

				// could the rest match the rest? 

				tail = slow(m, rest, stop, es, stopst);
				if (tail == stop)
					break;		// yes! 

				// no -- try a shorter match for this one 

				stp = rest - 1;
				ASSERT(stp >= sp);	// it did work 

			}
			ssub = ss + 1;
			esub = ss + OPND(m->g->strip[ss]) - 1;
			ASSERT(OP(m->g->strip[esub]) == OOR1);
			for (;;) {	// find first matching branch 

				if (slow(m, sp, rest, ssub, esub) == rest)
					break;	// it matched all of it 

				// that one missed, try next one 

				ASSERT(OP(m->g->strip[esub]) == OOR1);
				esub++;
				ASSERT(OP(m->g->strip[esub]) == OOR2);
				ssub = esub + 1;
				esub += OPND(m->g->strip[esub]);
				if (OP(m->g->strip[esub]) == OOR2)
					esub--;
				else
					ASSERT(OP(m->g->strip[esub]) == O_CH);
			}
			dp = dissect(m, sp, rest, ssub, esub);
			ASSERT(dp == rest);
			sp = rest;
			break;
		case O_PLUS:
		case O_QUEST:
		case OOR1:
		case OOR2:
		case O_CH:
			ASSERT(false);
			break;
		case OLPAREN:
			i = OPND(m->g->strip[ss]);
			ASSERT(0 < i && i <= (int)(m->g->nsub));
			m->pmatch[i].i = sp - m->offp;
			break;
		case ORPAREN:
			i = OPND(m->g->strip[ss]);
			ASSERT(0 < i && i <= (int)(m->g->nsub));
			m->pmatch[i].len = sp - m->offp;
			break;
		default:		// uh oh 

			ASSERT(false);
			break;
		}
	}

	ASSERT(sp == stop);
	return(sp);
}

/*
 - backref - figure out what matched what, figuring in back references
 == static char *backref(register struct match *m, char *start, \
 ==	char *stop, sopno startst, sopno stopst, sopno lev);
 */
static char *			// == stop (success) or NULL (failure) 

backref(register struct match *m, char *start, char *stop, sopno startst, sopno stopst, sopno lev)
	// PLUS nesting level 

{
	register int i;
	register sopno ss;	// start sop of current subRE 
	register char *sp;	// start of string matched by it 
	register sopno ssub;	// start sop of subsubRE 
	register sopno esub;	// end sop of subsubRE 
	register char *ssp;	// start of string matched by subsubRE 
	register char *dp;
	register size_t len;
	register int hard;
	register sop s;
	register regoff_t offsave;
	register cset *cs;

	AT("back", start, stop, startst, stopst);
	sp = start;

	// get as far as we can with easy stuff 

	hard = 0;
	for (ss = startst; !hard && ss < stopst; ss++)
		switch (OP(s = m->g->strip[ss])) {
		case OCHAR:
			if (sp == stop || *sp++ != (char)OPND(s))
				return(NULL);
			break;
		case OANY:
			if (sp == stop)
				return(NULL);
			sp++;
			break;
		case OANYOF:
			cs = &m->g->sets[OPND(s)];
			if (sp == stop || !CHIN(cs, *sp++))
				return(NULL);
			break;
		case OBOL:
			if ( (sp == m->beginp && !(m->eflags&REG_NOTBOL)) ||
					(sp < m->endp && *(sp-1) == '\n' &&
						(m->g->cflags&REG_SINGLELINE)) )
				{ /* yes */ }
			else
				return(NULL);
			break;
		case OEOL:
			if ( (sp == m->endp && !(m->eflags&REG_NOTEOL)) ||
					(sp < m->endp && *sp == '\n' &&
						(m->g->cflags&REG_SINGLELINE)) )
				{ /* yes */ }
			else
				return(NULL);
			break;
		case OBOW:
			if (( (sp == m->beginp && !(m->eflags&REG_NOTBOL)) ||
					(sp < m->endp && *(sp-1) == '\n' &&
						(m->g->cflags&REG_SINGLELINE)) ||
					(sp > m->beginp &&
							!ISWORD(*(sp-1))) ) &&
					(sp < m->endp && ISWORD(*sp)) )
				{ /* yes */ }
			else
				return(NULL);
			break;
		case OEOW:
			if (( (sp == m->endp && !(m->eflags&REG_NOTEOL)) ||
					(sp < m->endp && *sp == '\n' &&
						(m->g->cflags&REG_SINGLELINE)) ||
					(sp < m->endp && !ISWORD(*sp)) ) &&
					(sp > m->beginp && ISWORD(*(sp-1))) )
				{ /* yes */ }
			else
				return(NULL);
			break;
		case O_QUEST:
			break;
		case OOR1:	// matches null but needs to skip 

			ss++;
			s = m->g->strip[ss];
			do {
				ASSERT(OP(s) == OOR2);
				ss += OPND(s);
			} while (OP(s = m->g->strip[ss]) != O_CH);
			// note that the ss++ gets us past the O_CH 

			break;
		default:	// have to make a choice 

			hard = 1;
			break;
		}
	if (!hard) {		// that was it! 

		if (sp != stop)
			return(NULL);
		return(sp);
	}
	ss--;			// adjust for the for's final increment 


	// the hard stuff 

	AT("hard", sp, stop, ss, stopst);
	s = m->g->strip[ss];
	switch (OP(s)) {
	case OBACK_:		// the vilest depths 

		i = OPND(s);
		ASSERT(0 < i && i <= (int)(m->g->nsub));
		if (m->pmatch[i].len == -1)
			return(NULL);
		ASSERT(m->pmatch[i].i != -1);
		len = m->pmatch[i].len - m->pmatch[i].i;
		ASSERT(stop - m->beginp >= (int)(len));
		if (sp > stop - len)
			return(NULL);	// not enough left to match 

		ssp = m->offp + m->pmatch[i].i;
		if (memcmp(sp, ssp, len) != 0)
			return(NULL);
		while (m->g->strip[ss] != (sop)(SOP(O_BACK, i)))
			ss++;
		return(backref(m, sp+len, stop, ss+1, stopst, lev));
	case OQUEST_:		// to null or not

		dp = backref(m, sp, stop, ss+1, stopst, lev);
		if (dp)
			return(dp);	// not

		return(backref(m, sp, stop, ss+OPND(s)+1, stopst, lev));
	case OPLUS_:
		ASSERT(m->lastpos);
		ASSERT(lev+1 <= m->g->nplus);
		m->lastpos[lev+1] = sp;
		return(backref(m, sp, stop, ss+1, stopst, lev+1));
	case O_PLUS:
		if (sp == m->lastpos[lev])	// last pass matched null 

			return(backref(m, sp, stop, ss+1, stopst, lev-1));
		// try another pass 

		m->lastpos[lev] = sp;
		dp = backref(m, sp, stop, ss-OPND(s)+1, stopst, lev);
		if (!dp) return(backref(m, sp, stop, ss+1, stopst, lev-1));
		else return(dp);
	case OCH_:		// find the right one, if any

		ssub = ss + 1;
		esub = ss + OPND(s) - 1;
		ASSERT(OP(m->g->strip[esub]) == OOR1);
		for (;;) {	// find first matching branch 

			dp = backref(m, sp, stop, ssub, esub, lev);
			if (dp)
				return(dp);
			// that one missed, try next one 

			if (OP(m->g->strip[esub]) == O_CH)
				return(NULL);	// there is none 

			esub++;
			ASSERT(OP(m->g->strip[esub]) == OOR2);
			ssub = esub + 1;
			esub += OPND(m->g->strip[esub]);
			if (OP(m->g->strip[esub]) == OOR2)
				esub--;
			else
				ASSERT(OP(m->g->strip[esub]) == O_CH);
		}
	case OLPAREN:		// must undo assignment if rest fails

		i = OPND(s);
		ASSERT(0 < i && i <= (int)(m->g->nsub));
		offsave = m->pmatch[i].i;
		m->pmatch[i].i = sp - m->offp;
		dp = backref(m, sp, stop, ss+1, stopst, lev);
		if (dp)
			return(dp);
		m->pmatch[i].i = offsave;
		return(NULL);
	case ORPAREN:		// must undo assignment if rest fails

		i = OPND(s);
		ASSERT(0 < i && i <= (int)(m->g->nsub));
		offsave = m->pmatch[i].len;
		m->pmatch[i].len = sp - m->offp;
		dp = backref(m, sp, stop, ss+1, stopst, lev);
		if (dp)
			return(dp);
		m->pmatch[i].len = offsave;
		return(NULL);
	default:		// uh oh

		ASSERT(false);
		break;
	}

	// "can't happen" 

	ASSERT(false);
	// NOTREACHED 

	return NULL;
}

/*
 - fast - step through the string at top speed
 == static char *fast(register struct match *m, char *start, \
 ==	char *stop, sopno startst, sopno stopst);
 */
static char *			// where tentative match ended, or NULL 

fast(register struct match *m, char *start, char *stop, sopno startst, sopno stopst)
{
	register states st; // unused = m->st;
	register states fresh; // unused = m->fresh;
	register states tmp; // unused = m->tmp;
	register char *p = start;
	register int c = (start == m->beginp) ? REGOUT : *(start-1);
	register int lastc;	// previous c 

	register int flagch;
	register int i;
	register char *coldp;	// last p after which no match was underway 


	CLEAR(st);
	SET1(st, startst);
	st = step(m->g, startst, stopst, st, NOTHING, st);
	ASSIGN(fresh, st);
	SP("start", st, *p);
	coldp = NULL;
	for (;;) {
		// next character 

		lastc = c;
		c = (p == m->endp) ? REGOUT : *p;
		if (EQ(st, fresh))
			coldp = p;

		// is there an EOL and/or BOL between lastc and c? 

		flagch = '\0';
		i = 0;
		if ( (lastc == '\n' && m->g->cflags&REG_SINGLELINE) ||
				(lastc == REGOUT && !(m->eflags&REG_NOTBOL)) ) {
			flagch = BOL;
			i = m->g->nbol;
		}
		if ( (c == '\n' && m->g->cflags&REG_SINGLELINE) ||
				(c == REGOUT && !(m->eflags&REG_NOTEOL)) ) {
			flagch = (flagch == BOL) ? BOLEOL : EOL;
			i += m->g->neol;
		}
		if (i != 0) {
			for (; i > 0; i--)
				st = step(m->g, startst, stopst, st, flagch, st);
			SP("boleol", st, c);
		}

		// how about a word boundary? 

		if ( (flagch == BOL || (lastc != REGOUT && !ISWORD(lastc))) &&
					(c != REGOUT && ISWORD(c)) ) {
			flagch = BOW;
		}
		if ( (lastc != REGOUT && ISWORD(lastc)) &&
				(flagch == EOL || (c != REGOUT && !ISWORD(c))) ) {
			flagch = EOW;
		}
		if (flagch == BOW || flagch == EOW) {
			st = step(m->g, startst, stopst, st, flagch, st);
			SP("boweow", st, c);
		}

		// are we done? 

		if (ISSET(st, stopst) || p == stop)
			break;		// NOTE BREAK OUT 


		// no, we must deal with this character 

		ASSIGN(tmp, st);
		ASSIGN(st, fresh);
		ASSERT(c != REGOUT);
		st = step(m->g, startst, stopst, tmp, c, st);
		SP("aft", st, c);
		ASSERT(EQ(step(m->g, startst, stopst, st, NOTHING, st), st));
		p++;
	}

	ASSERT(coldp);
	m->coldp = coldp;
	if (ISSET(st, stopst))
		return(p+1);
	else
		return(NULL);
}

/*
 - slow - step through the string more deliberately
 == static char *slow(register struct match *m, char *start, \
 ==	char *stop, sopno startst, sopno stopst);
 */
static char *			// where it ended 

slow(register struct match *m, char *start, char *stop, sopno startst, sopno stopst)
{
	register states st; // unused = m->st;
	register states empty = m->empty;
	register states tmp; // unused = m->tmp;
	register char *p = start;
	register int c = (start == m->beginp) ? REGOUT : *(start-1);
	register int lastc;	// previous c 

	register int flagch;
	register int i;
	register char *matchp;	// last p at which a match ended 


	AT("slow", start, stop, startst, stopst);
	CLEAR(st);
	SET1(st, startst);
	SP("sstart", st, *p);
	st = step(m->g, startst, stopst, st, NOTHING, st);
	matchp = NULL;
	for (;;) {
		// next character 

		lastc = c;
		c = (p == m->endp) ? REGOUT : *p;

		// is there an EOL and/or BOL between lastc and c? 

		flagch = '\0';
		i = 0;
		if ( (lastc == '\n' && m->g->cflags&REG_SINGLELINE) ||
				(lastc == REGOUT && !(m->eflags&REG_NOTBOL)) ) {
			flagch = BOL;
			i = m->g->nbol;
		}
		if ( (c == '\n' && m->g->cflags&REG_SINGLELINE) ||
				(c == REGOUT && !(m->eflags&REG_NOTEOL)) ) {
			flagch = (flagch == BOL) ? BOLEOL : EOL;
			i += m->g->neol;
		}
		if (i != 0) {
			for (; i > 0; i--)
				st = step(m->g, startst, stopst, st, flagch, st);
			SP("sboleol", st, c);
		}

		// how about a word boundary? 

		if ( (flagch == BOL || (lastc != REGOUT && !ISWORD(lastc))) &&
					(c != REGOUT && ISWORD(c)) ) {
			flagch = BOW;
		}
		if ( (lastc != REGOUT && ISWORD(lastc)) &&
				(flagch == EOL || (c != REGOUT && !ISWORD(c))) ) {
			flagch = EOW;
		}
		if (flagch == BOW || flagch == EOW) {
			st = step(m->g, startst, stopst, st, flagch, st);
			SP("sboweow", st, c);
		}

		// are we done? 

		if (ISSET(st, stopst))
			matchp = p;
		if (EQ(st, empty) || p == stop)
			break;		// NOTE BREAK OUT 


		// no, we must deal with this character 

		ASSIGN(tmp, st);
		ASSIGN(st, empty);
		ASSERT(c != REGOUT);
		st = step(m->g, startst, stopst, tmp, c, st);
		SP("saft", st, c);
		ASSERT(EQ(step(m->g, startst, stopst, st, NOTHING, st), st));
		p++;
	}

	return(matchp);
}


/*
 - step - map set of states reachable before char to set reachable after
 == static states step(register struct re_guts *g, sopno start, sopno stop, \
 ==	register states bef, int ch, register states aft);
 == #define	BOL	(OUT+1)
 == #define	EOL	(BOL+1)
 == #define	BOLEOL	(BOL+2)
 == #define	NOTHING	(BOL+3)
 == #define	BOW	(BOL+4)
 == #define	EOW	(BOL+5)
 == #define	CODEMAX	(BOL+5)		// highest code used
 == #define	NONCHAR(c)	((c) > CHAR_MAX)
 == #define	NNONCHAR	(CODEMAX-CHAR_MAX)
 */
static states
step(register struct re_guts *g, sopno start, sopno stop, register states bef, int ch, register states aft)

	// start state within strip 

	// state after stop state within strip 

	// states reachable before 

	// character or NONCHAR code 

	// states already known reachable after 

{
	register cset *cs;
	register sop s;
	register sopno pc;
	register onestate here;		// note, macros know this name 

	register sopno look;
	register int i;

	for (pc = start, INIT(here, pc); pc != stop; pc++, INC(here)) {
		s = g->strip[pc];
		switch (OP(s)) {
		case OEND:
			ASSERT(pc == stop-1);
			break;
		case OCHAR:
			// only characters can match 

			ASSERT(!NONCHAR(ch) || ch != (char)OPND(s));
			if (ch == (char)OPND(s))
				FWD(aft, bef, 1);
			break;
		case OBOL:
			if (ch == BOL || ch == BOLEOL)
				FWD(aft, bef, 1);
			break;
		case OEOL:
			if (ch == EOL || ch == BOLEOL)
				FWD(aft, bef, 1);
			break;
		case OBOW:
			if (ch == BOW)
				FWD(aft, bef, 1);
			break;
		case OEOW:
			if (ch == EOW)
				FWD(aft, bef, 1);
			break;
		case OANY:
			if (!NONCHAR(ch))
				FWD(aft, bef, 1);
			break;
		case OANYOF:
			cs = &g->sets[OPND(s)];
			if (!NONCHAR(ch) && CHIN(cs, ch))
				FWD(aft, bef, 1);
			break;
		case OBACK_:		// ignored here 

		case O_BACK:
			FWD(aft, aft, 1);
			break;
		case OPLUS_:		// forward, this is just an empty 

			FWD(aft, aft, 1);
			break;
		case O_PLUS:		// both forward and back 

			FWD(aft, aft, 1);
			i = ISSETBACK(aft, OPND(s));
			BACK(aft, aft, OPND(s));
			if (!i && ISSETBACK(aft, OPND(s))) {
				// oho, must reconsider loop body 

				pc -= OPND(s) + 1;
				INIT(here, pc);
			}
			break;
		case OQUEST_:		// two branches, both forward 

			FWD(aft, aft, 1);
			FWD(aft, aft, OPND(s));
			break;
		case O_QUEST:		// just an empty 

			FWD(aft, aft, 1);
			break;
		case OLPAREN:		// not significant here 

		case ORPAREN:
			FWD(aft, aft, 1);
			break;
		case OCH_:		// mark the first two branches 

			FWD(aft, aft, 1);
			ASSERT(OP(g->strip[pc+OPND(s)]) == OOR2);
			FWD(aft, aft, OPND(s));
			break;
		case OOR1:		// done a branch, find the O_CH 

			if (ISSTATEIN(aft, here)) {
				for (look = 1;
						OP(s = g->strip[pc+look]) != O_CH;
						look += OPND(s))
					ASSERT(OP(s) == OOR2);
				FWD(aft, aft, look);
			}
			break;
		case OOR2:		// propagate OCH_'s marking 

			FWD(aft, aft, 1);
			if (OP(g->strip[pc+OPND(s)]) != O_CH) {
				ASSERT(OP(g->strip[pc+OPND(s)]) == OOR2);
				FWD(aft, aft, OPND(s));
			}
			break;
		case O_CH:		// just empty 

			FWD(aft, aft, 1);
			break;
		default:		// ooooops... 

			ASSERT(false);
			break;
		}
	}

	return(aft);
}

#undef	matcher
#undef	fast
#undef	slow
#undef	dissect
#undef	backref
#undef	step
#undef	print
#undef	at
#undef	match


// now undo things 

#undef	states
#undef	CLEAR
#undef	SET0
#undef	SET1
#undef	ISSET
#undef	ASSIGN
#undef	EQ
#undef	STATEVARS
#undef	STATESETUP
#undef	STATETEARDOWN
#undef	SETUP
#undef	onestate
#undef	INIT
#undef	INC
#undef	ISSTATEIN
#undef	FWD
#undef	BACK
#undef	ISSETBACK
#undef	SNAMES

// macros for manipulating states, large version 

#define	states	char *
#define	CLEAR(v)	memset(v, 0, m->g->nstates)
#define	SET0(v, n)	((v)[n] = 0)
#define	SET1(v, n)	((v)[n] = 1)
#define	ISSET(v, n)	((v)[n])
#define	ASSIGN(d, s)	memcpy(d, s, m->g->nstates)
#define	EQ(a, b)	(memcmp(a, b, m->g->nstates) == 0)
#define	STATEVARS	int vn; char *space
#define	STATESETUP(m, nv)	{ (m)->space = (char *)malloc((nv)*(m)->g->nstates); \
				if (!(m)->space) return(REG_ESPACE); \
				(m)->vn = 0; }
#define	STATETEARDOWN(m)	{ free((m)->space); }
#define	SETUP(v)	((v) = &m->space[m->vn++ * m->g->nstates])
#define	onestate	int
#define	INIT(o, n)	((o) = (n))
#define	INC(o)	((o)++)
#define	ISSTATEIN(v, o)	((v)[o])
// some abbreviations; note that some of these know variable names! 

// do "if I'm here, I can also be there" etc without branches 

#define	FWD(dst, src, n)	((dst)[here+(n)] |= (src)[here])
#define	BACK(dst, src, n)	((dst)[here-(n)] |= (src)[here])
#define	ISSETBACK(v, n)	((v)[here - (n)])
// function names 



////////////////////////////////////////////////////////////////////////////////
//	E N G I N E . C

#define	matcher	lmatcher
#define	fast	lfast
#define	slow	lslow
#define	dissect	ldissect
#define	backref	lbackref
#define	step	lstep
#define	print	lprint
#define	at	lat
#define	match	lmat

struct match {
	struct re_guts *g;
	int eflags;
	REGMATCH *pmatch;	// [nsub+1] (0 element unused) 
	char *offp;		// offsets work from here 
	char *beginp;		// start of string -- virtual NUL precedes 
	char *endp;		// end of string -- virtual NUL here 
	char *coldp;		// can be no match starting before here 
	char **lastpos;		// [nplus+1] 
	STATEVARS;
	states st;		// current states 
	states fresh;		// states for a fresh start 
	states tmp;		// temporary 
	states empty;		// empty set of states 
};

// === engine.c === 

static int matcher (struct re_guts *g, char *string, size_t nmatch, REGMATCH pmatch[], int eflags);
static char *dissect (struct match *m, char *start, char *stop, sopno startst, sopno stopst);
static char *backref (struct match *m, char *start, char *stop, sopno startst, sopno stopst, sopno lev);
static char *fast (struct match *m, char *start, char *stop, sopno startst, sopno stopst);
static char *slow (struct match *m, char *start, char *stop, sopno startst, sopno stopst);
static states step (struct re_guts *g, sopno start, sopno stop, states bef, int ch, states aft);
#define	BOL	(REGOUT+1)
#define	EOL	(BOL+1)
#define	BOLEOL	(BOL+2)
#define	NOTHING	(BOL+3)
#define	BOW	(BOL+4)
#define	EOW	(BOL+5)
#define	CODEMAX	(BOL+5)		// highest code used 

#define	NONCHAR(c)	((c) > CHAR_MAX)
#define	NNONCHAR	(CODEMAX-CHAR_MAX)

#define	SP(t, s, c)	// nothing 
#define	AT(t, p1, p2, s1, s2)	// nothing 
#define	NOTE(s)	// nothing 

/*
 - matcher - the actual matching engine
 == static int matcher(register struct re_guts *g, char *string, \
 ==	size_t nmatch, REGMATCH pmatch[], int eflags);
 */
static int			// 0 success, REG_NOMATCH failure 

matcher(register struct re_guts *g, char *string, size_t nmatch, REGMATCH pmatch[], int eflags)
{
	register char *endp;
	register int i;
	struct match mv;
	register struct match *m = &mv;
	register char *dp;
	const register sopno gf = g->firststate+1;	// +1 for OEND 

	const register sopno gl = g->laststate;
	char *start;
	char *stop;

	// simplify the situation where possible 

	if (g->cflags&REG_NOSUB)
		nmatch = 0;
	if (eflags&REG_STARTEND) {
		start = string + pmatch[0].i;
		stop = string + pmatch[0].len;
	} else {
		start = string;
		stop = start + strlen(start);
	}
	if (stop < start)
		return(REG_INVARG);

	// prescreening; this does wonders for this rather slow code 

	if (g->must) {
		for (dp = start; dp < stop; dp++)
			if (*dp == g->must[0] && stop - dp >= g->mlen &&
				memcmp(dp, g->must, (size_t)g->mlen) == 0)
				break;
		if (dp == stop)		// we didn't find g->must 

			return(REG_NOMATCH);
	}

	// match struct setup 

	m->g = g;
	m->eflags = eflags;
	m->pmatch = NULL;
	m->lastpos = NULL;
	m->offp = string;
	m->beginp = start;
	m->endp = stop;
	STATESETUP(m, 4);
	SETUP(m->st);
	SETUP(m->fresh);
	SETUP(m->tmp);
	SETUP(m->empty);
	CLEAR(m->empty);

	// this loop does only one repetition except for backrefs
	for (;;) {
		endp = fast(m, start, stop, gf, gl);
		if (!endp) {		// a miss 

			STATETEARDOWN(m);
			return(REG_NOMATCH);
		}
		if (nmatch == 0 && !g->backrefs)
			break;		// no further info needed 


		// where? 

		ASSERT(m->coldp);
		for (;;) {
			NOTE("finding start");
			endp = slow(m, m->coldp, stop, gf, gl);
			if (endp)
				break;
			ASSERT(m->coldp < m->endp);
			m->coldp++;
		}
		if (nmatch == 1 && !g->backrefs)
			break;		// no further info needed 


		// oh my, he wants the subexpressions... 

		if (!m->pmatch)
			m->pmatch = (REGMATCH *)malloc((m->g->nsub + 1) *
							sizeof(REGMATCH));
		if (!m->pmatch) {
			STATETEARDOWN(m);
			return(REG_ESPACE);
		}
		for (i = 1; i <= (int)(m->g->nsub); i++)
			m->pmatch[i].i = m->pmatch[i].len = -1;
		if (!g->backrefs && !(m->eflags&REG_BACKR)) {
			NOTE("dissecting");
			dp = dissect(m, m->coldp, endp, gf, gl);
		} else {
			if (g->nplus > 0 && !m->lastpos)
				m->lastpos = (char **)malloc((g->nplus+1) *
							sizeof(char *));
			if (g->nplus > 0 && !m->lastpos) {
				free(m->pmatch);
				STATETEARDOWN(m);
				return(REG_ESPACE);
			}
			NOTE("backref dissect");
			dp = backref(m, m->coldp, endp, gf, gl, (sopno)0);
		}
		if (dp)
			break;

		// uh-oh... we couldn't find a subexpression-level match 

		ASSERT(g->backrefs);	// must be back references doing it 

		ASSERT(g->nplus == 0 || m->lastpos);
		for (;;) {
			if (dp || endp <= m->coldp)
				break;		// defeat 

			NOTE("backoff");
			endp = slow(m, m->coldp, endp-1, gf, gl);
			if (!endp)
				break;		// defeat 

			// try it on a shorter possibility 

#ifndef NDEBUG
			for (i = 1; i <= (int)(m->g->nsub); i++) {
				ASSERT(m->pmatch[i].i == -1);
				ASSERT(m->pmatch[i].len == -1);
			}
#endif
			NOTE("backoff dissect");
			dp = backref(m, m->coldp, endp, gf, gl, (sopno)0);
		}
		ASSERT(!dp || dp == endp);
		if (dp)		// found a shorter one 

			break;

		// despite initial appearances, there is no match here 

		NOTE("false alarm");
		start = m->coldp + 1;	// recycle starting later 

		ASSERT(start <= stop);
	}

	// fill in the details if requested 

	if (nmatch > 0) {
		pmatch[0].i = m->coldp - m->offp;
		pmatch[0].len = endp - m->offp;
	}
	if (nmatch > 1) {
		ASSERT(m->pmatch);
		for (i = 1; i < (int)nmatch; i++)
			if (i <= (int)(m->g->nsub))
				pmatch[i] = m->pmatch[i];
			else {
				pmatch[i].i = -1;
				pmatch[i].len = -1;
			}
	}

	if (m->pmatch)
		free((char *)m->pmatch);
	if (m->lastpos)
		free((char *)m->lastpos);
	STATETEARDOWN(m);
	return(0);
}

/*
 - dissect - figure out what matched what, no back references
 == static char *dissect(register struct match *m, char *start, \
 ==	char *stop, sopno startst, sopno stopst);
 */
static char *			// == stop (success) always 

dissect(register struct match *m, char *start, char *stop, sopno startst, sopno stopst)
{
	register int i;
	register sopno ss;	// start sop of current subRE 
	register sopno es;	// end sop of current subRE 
	register char *sp;	// start of string matched by it 
	register char *stp;	// string matched by it cannot pass here 
	register char *rest;	// start of rest of string 
	register char *tail;	// string unmatched by rest of RE 
	register sopno ssub;	// start sop of subsubRE 
	register sopno esub;	// end sop of subsubRE 
	register char *ssp;	// start of string matched by subsubRE 
	register char *sep;	// end of string matched by subsubRE 
	register char *oldssp;	// previous ssp 
	register char *dp;
	AT("diss", start, stop, startst, stopst);
	sp = start;
	for (ss = startst; ss < stopst; ss = es) {
		// identify end of subRE 

		es = ss;
		switch (OP(m->g->strip[es])) {
		case OPLUS_:
		case OQUEST_:
			es += OPND(m->g->strip[es]);
			break;
		case OCH_:
			while (OP(m->g->strip[es]) != O_CH)
				es += OPND(m->g->strip[es]);
			break;
		}
		es++;

		// figure out what it matched 

		switch (OP(m->g->strip[ss])) {
		case OEND:
			ASSERT(false);
			break;
		case OCHAR:
			sp++;
			break;
		case OBOL:
		case OEOL:
		case OBOW:
		case OEOW:
			break;
		case OANY:
		case OANYOF:
			sp++;
			break;
		case OBACK_:
		case O_BACK:
			ASSERT(false);
			break;
		// cases where length of match is hard to find 

		case OQUEST_:
			stp = stop;
			for (;;) {
				// how long could this one be? 

				rest = slow(m, sp, stp, ss, es);
				ASSERT(rest);	// it did match 

				// could the rest match the rest? 

				tail = slow(m, rest, stop, es, stopst);
				if (tail == stop)
					break;		// yes! 

				// no -- try a shorter match for this one 

				stp = rest - 1;
				ASSERT(stp >= sp);	// it did work 

			}
			ssub = ss + 1;
			esub = es - 1;
			// did innards match? 

			if (slow(m, sp, rest, ssub, esub)) {
				dp = dissect(m, sp, rest, ssub, esub);
				ASSERT(dp == rest);
			} else		// no 

				ASSERT(sp == rest);
			sp = rest;
			break;
		case OPLUS_:
			stp = stop;
			for (;;) {
				// how long could this one be? 

				rest = slow(m, sp, stp, ss, es);
				ASSERT(rest);	// it did match 

				// could the rest match the rest? 

				tail = slow(m, rest, stop, es, stopst);
				if (tail == stop)
					break;		// yes! 

				// no -- try a shorter match for this one 

				stp = rest - 1;
				ASSERT(stp >= sp);	// it did work 

			}
			ssub = ss + 1;
			esub = es - 1;
			ssp = sp;
			oldssp = ssp;
			for (;;) {	// find last match of innards 

				sep = slow(m, ssp, rest, ssub, esub);
				if (!sep || sep == ssp)
					break;	// failed or matched null 

				oldssp = ssp;	// on to next try 

				ssp = sep;
			}
			if (!sep) {
				// last successful match 

				sep = ssp;
				ssp = oldssp;
			}
			ASSERT(sep == rest);	// must exhaust substring 

			ASSERT(slow(m, ssp, sep, ssub, esub) == rest);
			dp = dissect(m, ssp, sep, ssub, esub);
			ASSERT(dp == sep);
			sp = rest;
			break;
		case OCH_:
			stp = stop;
			for (;;) {
				// how long could this one be? 

				rest = slow(m, sp, stp, ss, es);
				ASSERT(rest);	// it did match 

				// could the rest match the rest? 

				tail = slow(m, rest, stop, es, stopst);
				if (tail == stop)
					break;		// yes! 

				// no -- try a shorter match for this one 

				stp = rest - 1;
				ASSERT(stp >= sp);	// it did work 

			}
			ssub = ss + 1;
			esub = ss + OPND(m->g->strip[ss]) - 1;
			ASSERT(OP(m->g->strip[esub]) == OOR1);
			for (;;) {	// find first matching branch 

				if (slow(m, sp, rest, ssub, esub) == rest)
					break;	// it matched all of it 

				// that one missed, try next one 

				ASSERT(OP(m->g->strip[esub]) == OOR1);
				esub++;
				ASSERT(OP(m->g->strip[esub]) == OOR2);
				ssub = esub + 1;
				esub += OPND(m->g->strip[esub]);
				if (OP(m->g->strip[esub]) == OOR2)
					esub--;
				else
					ASSERT(OP(m->g->strip[esub]) == O_CH);
			}
			dp = dissect(m, sp, rest, ssub, esub);
			ASSERT(dp == rest);
			sp = rest;
			break;
		case O_PLUS:
		case O_QUEST:
		case OOR1:
		case OOR2:
		case O_CH:
			ASSERT(false);
			break;
		case OLPAREN:
			i = OPND(m->g->strip[ss]);
			ASSERT(0 < i && i <= (int)(m->g->nsub));
			m->pmatch[i].i = sp - m->offp;
			break;
		case ORPAREN:
			i = OPND(m->g->strip[ss]);
			ASSERT(0 < i && i <= (int)(m->g->nsub));
			m->pmatch[i].len = sp - m->offp;
			break;
		default:		// uh oh 

			ASSERT(false);
			break;
		}
	}

	ASSERT(sp == stop);
	return(sp);
}

/*
 - backref - figure out what matched what, figuring in back references
 == static char *backref(register struct match *m, char *start, \
 ==	char *stop, sopno startst, sopno stopst, sopno lev);
 */
static char *			// == stop (success) or NULL (failure) 

backref(register struct match *m, char *start, char *stop, sopno startst, sopno stopst, sopno lev)
	// PLUS nesting level 

{
	register int i;
	register sopno ss;	// start sop of current subRE 
	register char *sp;	// start of string matched by it 
	register sopno ssub;	// start sop of subsubRE 
	register sopno esub;	// end sop of subsubRE 
	register char *ssp;	// start of string matched by subsubRE 
	register char *dp;
	register size_t len;
	register int hard;
	register sop s;
	register regoff_t offsave;
	register cset *cs;

	AT("back", start, stop, startst, stopst);
	sp = start;

	// get as far as we can with easy stuff 

	hard = 0;
	for (ss = startst; !hard && ss < stopst; ss++)
		switch (OP(s = m->g->strip[ss])) {
		case OCHAR:
			if (sp == stop || *sp++ != (char)OPND(s))
				return(NULL);
			break;
		case OANY:
			if (sp == stop)
				return(NULL);
			sp++;
			break;
		case OANYOF:
			cs = &m->g->sets[OPND(s)];
			if (sp == stop || !CHIN(cs, *sp++))
				return(NULL);
			break;
		case OBOL:
			if ( (sp == m->beginp && !(m->eflags&REG_NOTBOL)) ||
					(sp < m->endp && *(sp-1) == '\n' &&
						(m->g->cflags&REG_SINGLELINE)) )
				{ /* yes */ }
			else
				return(NULL);
			break;
		case OEOL:
			if ( (sp == m->endp && !(m->eflags&REG_NOTEOL)) ||
					(sp < m->endp && *sp == '\n' &&
						(m->g->cflags&REG_SINGLELINE)) )
				{ /* yes */ }
			else
				return(NULL);
			break;
		case OBOW:
			if (( (sp == m->beginp && !(m->eflags&REG_NOTBOL)) ||
					(sp < m->endp && *(sp-1) == '\n' &&
						(m->g->cflags&REG_SINGLELINE)) ||
					(sp > m->beginp &&
							!ISWORD(*(sp-1))) ) &&
					(sp < m->endp && ISWORD(*sp)) )
				{ /* yes */ }
			else
				return(NULL);
			break;
		case OEOW:
			if (( (sp == m->endp && !(m->eflags&REG_NOTEOL)) ||
					(sp < m->endp && *sp == '\n' &&
						(m->g->cflags&REG_SINGLELINE)) ||
					(sp < m->endp && !ISWORD(*sp)) ) &&
					(sp > m->beginp && ISWORD(*(sp-1))) )
				{ /* yes */ }
			else
				return(NULL);
			break;
		case O_QUEST:
			break;
		case OOR1:	// matches null but needs to skip 

			ss++;
			s = m->g->strip[ss];
			do {
				ASSERT(OP(s) == OOR2);
				ss += OPND(s);
			} while (OP(s = m->g->strip[ss]) != O_CH);
			// note that the ss++ gets us past the O_CH 

			break;
		default:	// have to make a choice 

			hard = 1;
			break;
		}
	if (!hard) {		// that was it! 

		if (sp != stop)
			return(NULL);
		return(sp);
	}
	ss--;			// adjust for the for's final increment 


	// the hard stuff 

	AT("hard", sp, stop, ss, stopst);
	s = m->g->strip[ss];
	switch (OP(s)) {
	case OBACK_:		// the vilest depths 

		i = OPND(s);
		ASSERT(0 < i && i <= (int)(m->g->nsub));
		if (m->pmatch[i].len == -1)
			return(NULL);
		ASSERT(m->pmatch[i].i != -1);
		len = m->pmatch[i].len - m->pmatch[i].i;
		ASSERT(stop - m->beginp >= (int)(len));
		if (sp > stop - len)
			return(NULL);	// not enough left to match 

		ssp = m->offp + m->pmatch[i].i;
		if (memcmp(sp, ssp, len) != 0)
			return(NULL);
		while (m->g->strip[ss] != (sop)(SOP(O_BACK, i)))
			ss++;
		return(backref(m, sp+len, stop, ss+1, stopst, lev));
	case OQUEST_:		// to null or not

		dp = backref(m, sp, stop, ss+1, stopst, lev);
		if (dp)
			return(dp);	// not 

		return(backref(m, sp, stop, ss+OPND(s)+1, stopst, lev));
	case OPLUS_:
		ASSERT(m->lastpos);
		ASSERT(lev+1 <= m->g->nplus);
		m->lastpos[lev+1] = sp;
		return(backref(m, sp, stop, ss+1, stopst, lev+1));
	case O_PLUS:
		if (sp == m->lastpos[lev])	// last pass matched null 

			return(backref(m, sp, stop, ss+1, stopst, lev-1));
		// try another pass 

		m->lastpos[lev] = sp;
		dp = backref(m, sp, stop, ss-OPND(s)+1, stopst, lev);
		if (!dp) return(backref(m, sp, stop, ss+1, stopst, lev-1));
		else return(dp);
	case OCH_:		// find the right one, if any

		ssub = ss + 1;
		esub = ss + OPND(s) - 1;
		ASSERT(OP(m->g->strip[esub]) == OOR1);
		for (;;) {	// find first matching branch 

			dp = backref(m, sp, stop, ssub, esub, lev);
			if (dp)
				return(dp);
			// that one missed, try next one 

			if (OP(m->g->strip[esub]) == O_CH)
				return(NULL);	// there is none 

			esub++;
			ASSERT(OP(m->g->strip[esub]) == OOR2);
			ssub = esub + 1;
			esub += OPND(m->g->strip[esub]);
			if (OP(m->g->strip[esub]) == OOR2)
				esub--;
			else
				ASSERT(OP(m->g->strip[esub]) == O_CH);
		}
	case OLPAREN:		// must undo assignment if rest fails

		i = OPND(s);
		ASSERT(0 < i && i <= (int)(m->g->nsub));
		offsave = m->pmatch[i].i;
		m->pmatch[i].i = sp - m->offp;
		dp = backref(m, sp, stop, ss+1, stopst, lev);
		if (dp)
			return(dp);
		m->pmatch[i].i = offsave;
		return(NULL);
	case ORPAREN:		// must undo assignment if rest fails

		i = OPND(s);
		ASSERT(0 < i && i <= (int)(m->g->nsub));
		offsave = m->pmatch[i].len;
		m->pmatch[i].len = sp - m->offp;
		dp = backref(m, sp, stop, ss+1, stopst, lev);
		if (dp)
			return(dp);
		m->pmatch[i].len = offsave;
		return(NULL);
	default:		// uh oh 

		ASSERT(false);
		break;
	}

	// "can't happen" 

	ASSERT(false);
	// NOTREACHED 

	return NULL;
}

/*
 - fast - step through the string at top speed
 == static char *fast(register struct match *m, char *start, \
 ==	char *stop, sopno startst, sopno stopst);
 */
static char *			// where tentative match ended, or NULL 

fast(register struct match *m, char *start, char *stop, sopno startst, sopno stopst)
{
	register states st = m->st;
	register states fresh = m->fresh;
	register states tmp = m->tmp;
	register char *p = start;
	register int c = (start == m->beginp) ? REGOUT : *(start-1);
	register int lastc;	// previous c 

	register int flagch;
	register int i;
	register char *coldp;	// last p after which no match was underway 


	CLEAR(st);
	SET1(st, startst);
	st = step(m->g, startst, stopst, st, NOTHING, st);
	ASSIGN(fresh, st);
	SP("start", st, *p);
	coldp = NULL;
	for (;;) {
		// next character 

		lastc = c;
		c = (p == m->endp) ? REGOUT : *p;
		if (EQ(st, fresh))
			coldp = p;

		// is there an EOL and/or BOL between lastc and c? 

		flagch = '\0';
		i = 0;
		if ( (lastc == '\n' && m->g->cflags&REG_SINGLELINE) ||
				(lastc == REGOUT && !(m->eflags&REG_NOTBOL)) ) {
			flagch = BOL;
			i = m->g->nbol;
		}
		if ( (c == '\n' && m->g->cflags&REG_SINGLELINE) ||
				(c == REGOUT && !(m->eflags&REG_NOTEOL)) ) {
			flagch = (flagch == BOL) ? BOLEOL : EOL;
			i += m->g->neol;
		}
		if (i != 0) {
			for (; i > 0; i--)
				st = step(m->g, startst, stopst, st, flagch, st);
			SP("boleol", st, c);
		}

		// how about a word boundary? 

		if ( (flagch == BOL || (lastc != REGOUT && !ISWORD(lastc))) &&
					(c != REGOUT && ISWORD(c)) ) {
			flagch = BOW;
		}
		if ( (lastc != REGOUT && ISWORD(lastc)) &&
				(flagch == EOL || (c != REGOUT && !ISWORD(c))) ) {
			flagch = EOW;
		}
		if (flagch == BOW || flagch == EOW) {
			st = step(m->g, startst, stopst, st, flagch, st);
			SP("boweow", st, c);
		}

		// are we done? 

		if (ISSET(st, stopst) || p == stop)
			break;		// NOTE BREAK OUT 


		// no, we must deal with this character 

		ASSIGN(tmp, st);
		ASSIGN(st, fresh);
		ASSERT(c != REGOUT);
		st = step(m->g, startst, stopst, tmp, c, st);
		SP("aft", st, c);
		ASSERT(EQ(step(m->g, startst, stopst, st, NOTHING, st), st));
		p++;
	}

	ASSERT(coldp);
	m->coldp = coldp;
	if (ISSET(st, stopst))
		return(p+1);
	else
		return(NULL);
}

/*
 - slow - step through the string more deliberately
 == static char *slow(register struct match *m, char *start, \
 ==	char *stop, sopno startst, sopno stopst);
 */
static char *			// where it ended 

slow(register struct match *m, char *start, char *stop, sopno startst, sopno stopst)
{
	register states st = m->st;
	register states empty = m->empty;
	register states tmp = m->tmp;
	register char *p = start;
	register int c = (start == m->beginp) ? REGOUT : *(start-1);
	register int lastc;	// previous c 

	register int flagch;
	register int i;
	register char *matchp;	// last p at which a match ended 


	AT("slow", start, stop, startst, stopst);
	CLEAR(st);
	SET1(st, startst);
	SP("sstart", st, *p);
	st = step(m->g, startst, stopst, st, NOTHING, st);
	matchp = NULL;
	for (;;) {
		// next character 

		lastc = c;
		c = (p == m->endp) ? REGOUT : *p;

		// is there an EOL and/or BOL between lastc and c? 

		flagch = '\0';
		i = 0;
		if ( (lastc == '\n' && m->g->cflags&REG_SINGLELINE) ||
				(lastc == REGOUT && !(m->eflags&REG_NOTBOL)) ) {
			flagch = BOL;
			i = m->g->nbol;
		}
		if ( (c == '\n' && m->g->cflags&REG_SINGLELINE) ||
				(c == REGOUT && !(m->eflags&REG_NOTEOL)) ) {
			flagch = (flagch == BOL) ? BOLEOL : EOL;
			i += m->g->neol;
		}
		if (i != 0) {
			for (; i > 0; i--)
				st = step(m->g, startst, stopst, st, flagch, st);
			SP("sboleol", st, c);
		}

		// how about a word boundary? 

		if ( (flagch == BOL || (lastc != REGOUT && !ISWORD(lastc))) &&
					(c != REGOUT && ISWORD(c)) ) {
			flagch = BOW;
		}
		if ( (lastc != REGOUT && ISWORD(lastc)) &&
				(flagch == EOL || (c != REGOUT && !ISWORD(c))) ) {
			flagch = EOW;
		}
		if (flagch == BOW || flagch == EOW) {
			st = step(m->g, startst, stopst, st, flagch, st);
			SP("sboweow", st, c);
		}

		// are we done? 

		if (ISSET(st, stopst))
			matchp = p;
		if (EQ(st, empty) || p == stop)
			break;		// NOTE BREAK OUT 


		// no, we must deal with this character 

		ASSIGN(tmp, st);
		ASSIGN(st, empty);
		ASSERT(c != REGOUT);
		st = step(m->g, startst, stopst, tmp, c, st);
		SP("saft", st, c);
		ASSERT(EQ(step(m->g, startst, stopst, st, NOTHING, st), st));
		p++;
	}

	return(matchp);
}


/*
 - step - map set of states reachable before char to set reachable after
 == static states step(register struct re_guts *g, sopno start, sopno stop, \
 ==	register states bef, int ch, register states aft);
 == #define	BOL	(OUT+1)
 == #define	EOL	(BOL+1)
 == #define	BOLEOL	(BOL+2)
 == #define	NOTHING	(BOL+3)
 == #define	BOW	(BOL+4)
 == #define	EOW	(BOL+5)
 == #define	CODEMAX	(BOL+5)		// highest code used
 == #define	NONCHAR(c)	((c) > CHAR_MAX)
 == #define	NNONCHAR	(CODEMAX-CHAR_MAX)
 */
static states
step(register struct re_guts *g, sopno start, sopno stop, register states bef, int ch, register states aft)

	// start state within strip 

	// state after stop state within strip 

	// states reachable before 

	// character or NONCHAR code 

	// states already known reachable after 

{
	register cset *cs;
	register sop s;
	register sopno pc;
	register onestate here;		// note, macros know this name 

	register sopno look;
	register int i;

	for (pc = start, INIT(here, pc); pc != stop; pc++, INC(here)) {
		s = g->strip[pc];
		switch (OP(s)) {
		case OEND:
			ASSERT(pc == stop-1);
			break;
		case OCHAR:
			// only characters can match 

			ASSERT(!NONCHAR(ch) || ch != (char)OPND(s));
			if (ch == (char)OPND(s))
				FWD(aft, bef, 1);
			break;
		case OBOL:
			if (ch == BOL || ch == BOLEOL)
				FWD(aft, bef, 1);
			break;
		case OEOL:
			if (ch == EOL || ch == BOLEOL)
				FWD(aft, bef, 1);
			break;
		case OBOW:
			if (ch == BOW)
				FWD(aft, bef, 1);
			break;
		case OEOW:
			if (ch == EOW)
				FWD(aft, bef, 1);
			break;
		case OANY:
			if (!NONCHAR(ch))
				FWD(aft, bef, 1);
			break;
		case OANYOF:
			cs = &g->sets[OPND(s)];
			if (!NONCHAR(ch) && CHIN(cs, ch))
				FWD(aft, bef, 1);
			break;
		case OBACK_:		// ignored here 

		case O_BACK:
			FWD(aft, aft, 1);
			break;
		case OPLUS_:		// forward, this is just an empty 

			FWD(aft, aft, 1);
			break;
		case O_PLUS:		// both forward and back 

			FWD(aft, aft, 1);
			i = ISSETBACK(aft, OPND(s));
			BACK(aft, aft, OPND(s));
			if (!i && ISSETBACK(aft, OPND(s))) {
				// oho, must reconsider loop body 

				pc -= OPND(s) + 1;
				INIT(here, pc);
			}
			break;
		case OQUEST_:		// two branches, both forward 

			FWD(aft, aft, 1);
			FWD(aft, aft, OPND(s));
			break;
		case O_QUEST:		// just an empty 

			FWD(aft, aft, 1);
			break;
		case OLPAREN:		// not significant here 

		case ORPAREN:
			FWD(aft, aft, 1);
			break;
		case OCH_:		// mark the first two branches 

			FWD(aft, aft, 1);
			ASSERT(OP(g->strip[pc+OPND(s)]) == OOR2);
			FWD(aft, aft, OPND(s));
			break;
		case OOR1:		// done a branch, find the O_CH 

			if (ISSTATEIN(aft, here)) {
				for (look = 1;
						OP(s = g->strip[pc+look]) != O_CH;
						look += OPND(s))
					ASSERT(OP(s) == OOR2);
				FWD(aft, aft, look);
			}
			break;
		case OOR2:		// propagate OCH_'s marking 

			FWD(aft, aft, 1);
			if (OP(g->strip[pc+OPND(s)]) != O_CH) {
				ASSERT(OP(g->strip[pc+OPND(s)]) == OOR2);
				FWD(aft, aft, OPND(s));
			}
			break;
		case O_CH:		// just empty 

			FWD(aft, aft, 1);
			break;
		default:		// ooooops... 

			ASSERT(false);
			break;
		}
	}

	return(aft);
}

#undef	matcher
#undef	fast
#undef	slow
#undef	dissect
#undef	backref
#undef	step
#undef	print
#undef	at
#undef	match


/*
 - regexec - interface for matching
 = extern int regexec(const regex_it *, const char *, size_t, \
 =					REGMATCH [], int);
 = #define	REG_NOTBOL	00001
 = #define	REG_NOTEOL	00002
 = #define	REG_STARTEND	00004
 = #define	REG_LARGE	01000	// force large representation
 = #define	REG_BACKR	02000	// force use of backref code
 *
 * We put this here so we can exploit knowledge of the state representation
 * when choosing which matcher to call.  Also, by this point the matchers
 * have been prototyped.
 */
int	regexec(const regex_it *preg, const char *string, size_t nmatch, REGMATCH pmatch[], int eflags)
{
	register struct re_guts *g = preg->re_g;

	if (preg->re_magic != MAGIC1 || g->magic != MAGIC2)
		return(REG_BADPAT);

	ASSERT(!(g->iflags&BAD));
	if (g->iflags & BAD)		// backstop for no-debug case
		return(REG_BADPAT);
	eflags = ((eflags)&(REG_NOTBOL|REG_NOTEOL|REG_STARTEND));

	if (g->nstates <= (sopno)(CHAR_BIT*sizeof(states1) && !(eflags&REG_LARGE)))
		return(smatcher(g, (char *)string, nmatch, pmatch, eflags));
	else
		return(lmatcher(g, (char *)string, nmatch, pmatch, eflags));
}


////////////////////////////////////////////////////////////////////////////////



// parse structure, passed up and down to avoid global variables and other clumsinesses

struct parse
{
	char *next;		// next character in RE 
	char *end;		// end of string (-> NUL normally) 
	int error;		// has an error been seen? 
	sop *strip;		// malloced strip 
	sopno ssize;		// malloced strip size (allocated) 
	sopno slen;		// malloced strip length (used) 
	int ncsalloc;		// number of csets allocated 
	struct re_guts *g;
#define	NPAREN	10	// we need to remember () 1-9 for back refs 
	sopno pbegin[NPAREN];	// -> ( ([0] unused) 
	sopno pend[NPAREN];	// -> ) ([0] unused) 
};

// ========= begin header generated by ./mkh ========= 


// === regcomp.c === 

static void p_ere (struct parse *p, int stop);
static void p_ere_exp (struct parse *p);
static int p_count (struct parse *p);
static void p_bracket (struct parse *p);
static void p_b_term (struct parse *p, cset *cs);
static void p_b_cclass (struct parse *p, cset *cs);
static void p_b_eclass (struct parse *p, cset *cs);
static char p_b_symbol (struct parse *p);
static char p_b_coll_elem (struct parse *p, int endc);
static char othercase (int ch);
static void bothcases (struct parse *p, int ch);
static void ordinary (struct parse *p, int ch);
static void nonnewline (struct parse *p);
static void repeat (struct parse *p, sopno start, int from, int to);
static int seterr (struct parse *p, int e);
static cset *allocset (struct parse *p);
static void freeset (struct parse *p, cset *cs);
static int freezeset (struct parse *p, cset *cs);
static int firstch (struct parse *p, cset *cs);
static int nch (struct parse *p, cset *cs);
static void mcadd (struct parse *p, cset *cs, char *cp);
//static void mcsub (cset *cs, char *cp);
//static int mcin (cset *cs, char *cp);
//static char *mcfind (cset *cs, char *cp);
static void mcinvert (struct parse *p, cset *cs);
static void mccase (struct parse *p, cset *cs);
static int isinsets (struct re_guts *g, int c);
static int samesets (struct re_guts *g, int c1, int c2);
static void categorize (struct parse *p, struct re_guts *g);
static sopno dupl (struct parse *p, sopno start, sopno finish);
static void doemit (struct parse *p, sop op, size_t opnd);
static void doinsert (struct parse *p, sop op, size_t opnd, sopno pos);
static void dofwd (struct parse *p, sopno pos, sop value);
static void enlarge (struct parse *p, sopno size);
static void stripsnug (struct parse *p, struct re_guts *g);
static void findmust (struct parse *p, struct re_guts *g);
static sopno pluscount (struct parse *p, struct re_guts *g);

static char nuls[10];		// place to point scanner in event of error 


/*
 * macros for use with parse structure
 * BEWARE:  these know that the parse structure is named `p' !!!
 */
#define	PEEK()	(*p->next)
#define	PEEK2()	(*(p->next+1))
#define	MORE()	(p->next < p->end)
#define	MORE2()	(p->next+1 < p->end)
#define	SEE(c)	(MORE() && PEEK() == (c))
#define	SEETWO(a, b)	(MORE() && MORE2() && PEEK() == (a) && PEEK2() == (b))
#define	EAT(c)	((SEE(c)) ? (NEXT(), 1) : 0)
#define	EATTWO(a, b)	((SEETWO(a, b)) ? (NEXT2(), 1) : 0)
#define	NEXT()	(p->next++)
#define	NEXT2()	(p->next += 2)
#define	NEXTn(n)	(p->next += (n))
#define	GETNEXT()	(*p->next++)
#define	SETERROR(e)	seterr(p, (e))
#define	REQUIRE(co, e)	((co) || SETERROR(e))
#define	MUSTSEE(c, e)	(REQUIRE(MORE() && PEEK() == (c), e))
#define	MUSTEAT(c, e)	(REQUIRE(MORE() && GETNEXT() == (c), e))
#define	MUSTNOTSEE(c, e)	(REQUIRE(!MORE() || PEEK() != (c), e))
#define	EMIT(op, sopnd)	doemit(p, (sop)(op), (size_t)(sopnd))
#define	INSERT(op, pos)	doinsert(p, (sop)(op), HERE()-(pos)+1, pos)
#define	AHEAD(pos)		dofwd(p, pos, HERE()-(pos))
#define	ASTERN(sop, pos)	EMIT(sop, HERE()-pos)
#define	HERE()		(p->slen)
#define	THERE()		(p->slen - 1)
#define	THERETHERE()	(p->slen - 2)
#define	DROP(n)	(p->slen -= (n))

/*
 - regcomp - interface for parser and compilation
 = extern int regcomp(regex_it *, const char *, int);
 = #define	REG_BASIC	0000
 = #define	REG_EXTENDED	0001
 = #define	REG_ICASE	0002
 = #define	REG_NOSUB	0004
 = #define	REG_SINGLELINE	0010
 = #define	REG_NOSPEC	0020
 = #define	REG_PEND	0040
 */

int	regcomp(regex_it *preg, const char *pattern, int cflags)	// 0 success, otherwise REG_something 

{
	struct parse pa;
	register struct re_guts *g;
	register struct parse *p = &pa;
	register int i;
	register size_t len;

	if (cflags&REG_PEND)
	{
		if (preg->re_endp < pattern)
			return(REG_INVARG);

		len = preg->re_endp - pattern;
	}
	else
		len = strlen((char *)pattern);

	// do the mallocs early so failure handling is easy
  g = (struct re_guts *)malloc(sizeof(struct re_guts) + (NC-1)*sizeof(cat_t));
	if (!g) return(REG_ESPACE);

	p->ssize = (sopno)(len/(size_t)2*(size_t)3 + (size_t)1);	// ugh
	p->strip = (sop *)malloc(p->ssize * sizeof(sop));
	p->slen = 0;
	if (!p->strip) {
		free((char *)g);
		return(REG_ESPACE);
	}

	// set things up 

	p->g = g;
	p->next = (char *)pattern;	// convenience; we do not modify it 

	p->end = p->next + len;
	p->error = 0;
	p->ncsalloc = 0;

	for (i = 0; i < NPAREN; i++)
	{
		p->pbegin[i] = 0;
		p->pend[i] = 0;
	}

	g->csetsize = NC;
	g->sets = NULL;
	g->setbits = NULL;
	g->ncsets = 0;
	g->cflags = cflags;
	g->iflags = 0;
	g->nbol = 0;
	g->neol = 0;
	g->must = NULL;
	g->mlen = 0;
	g->nsub = 0;
	g->ncategories = 1;	// category 0 is "everything else"
	g->categories = &g->catspace[-(CHAR_MIN)];
	(void) memset((char *)g->catspace, 0, NC*sizeof(cat_t));
	g->backrefs = 0;

	// do it
	EMIT(OEND, 0);
	g->firststate = THERE();
	p_ere(p, REGOUT);
	EMIT(OEND, 0);
	g->laststate = THERE();

	// tidy up loose ends and fill things in
	categorize(p, g);
	stripsnug(p, g);
	findmust(p, g);
	g->nplus = pluscount(p, g);
	g->magic = MAGIC2;
	preg->re_nsub = g->nsub;
	preg->re_g = g;
	preg->re_magic = MAGIC1;

	// not debugging, so can't rely on the ASSERT() in regexec()
	if (g->iflags&BAD) SETERROR(REG_ASSERT);

	// win or lose, we're done 

	if (p->error != 0) regfree(preg);
	return(p->error);
}

// p_ere - ERE parser top level, concatenation and alternation
static void p_ere(struct parse *p, int stop)	// stop is character this ERE should end at 

{
	register char c;
	register sopno prevback=0;
	register sopno prevfwd=0;
	register sopno conc;
	register int first = 1;		// is this the first alternative? 


	for (;;) {
		// do a bunch of concatenated expressions 

		conc = HERE();
		while (MORE() && (c = PEEK()) != '|' && c != stop)
			p_ere_exp(p);
		REQUIRE(HERE() != conc, REG_EMPTY);	// require nonempty 


		if (!EAT('|'))
			break;		// NOTE BREAK OUT 


		if (first) {
			INSERT(OCH_, conc);	// offset is wrong 

			prevfwd = conc;
			prevback = conc;
			first = 0;
		}
		ASTERN(OOR1, prevback);
		prevback = THERE();
		AHEAD(prevfwd);			// fix previous offset 

		prevfwd = HERE();
		EMIT(OOR2, 0);			// offset is very wrong 

	}

	if (!first) {		// tail-end fixups 

		AHEAD(prevfwd);
		ASTERN(O_CH, prevback);
	}

	ASSERT(!MORE() || SEE(stop));
}

/*
 - p_ere_exp - parse one subERE, an atom possibly followed by a repetition op
 == static void p_ere_exp(register struct parse *p);
 */
static void
p_ere_exp(register struct parse *p)
{
	register char c;
	register sopno pos;
	register int count;
	register int count2;
	register sopno subno;
	int wascaret = 0;

	ASSERT(MORE());		// caller should have ensured this 

	c = GETNEXT();

	pos = HERE();
	switch (c) {
	case '(':
		REQUIRE(MORE(), REG_EPAREN);
		p->g->nsub++;
		subno = (sopno)(p->g->nsub);
		if (subno < NPAREN)
			p->pbegin[subno] = HERE();
		EMIT(OLPAREN, subno);
		if (!SEE(')'))
			p_ere(p, ')');
		if (subno < NPAREN) {
			p->pend[subno] = HERE();
			ASSERT(p->pend[subno] != 0);
		}
		EMIT(ORPAREN, subno);
		MUSTEAT(')', REG_EPAREN);
		break;
#ifndef POSIX_MISTAKE
	case ')':		// happens only if no current unmatched ( 

		/*
		 * You may ask, why the ifndef?  Because I didn't notice
		 * this until slightly too late for 1003.2, and none of the
		 * other 1003.2 regular-expression reviewers noticed it at
		 * all.  So an unmatched ) is legal POSIX, at least until
		 * we can get it fixed.
		 */
		SETERROR(REG_EPAREN);
		break;
#endif
	case '^':
		EMIT(OBOL, 0);
		p->g->iflags |= USEBOL;
		p->g->nbol++;
		wascaret = 1;
		break;
	case '$':
		EMIT(OEOL, 0);
		p->g->iflags |= USEEOL;
		p->g->neol++;
		break;
	case '|':
		SETERROR(REG_EMPTY);
		break;
	case '*':
	case '+':
	case '?':
		SETERROR(REG_BADRPT);
		break;
	case '.':
		if (p->g->cflags&REG_SINGLELINE)
			nonnewline(p);
		else
			EMIT(OANY, 0);
		break;
	case '[':
		p_bracket(p);
		break;
	case '\\':
		REQUIRE(MORE(), REG_EESCAPE);
		c = GETNEXT();
		ordinary(p, c);
		break;
	case '{':		// okay as ordinary except if digit follows 

		REQUIRE(!MORE() || !isdigit(PEEK()), REG_BADRPT);
		// FALLTHROUGH 

	default:
		ordinary(p, c);
		break;
	}

	if (!MORE())
		return;
	c = PEEK();
	// we call { a repetition if followed by a digit 

	if (!( c == '*' || c == '+' || c == '?' ||
				(c == '{' && MORE2() && isdigit(PEEK2())) ))
		return;		// no repetition, we're done 

	NEXT();

	REQUIRE(!wascaret, REG_BADRPT);
	switch (c) {
	case '*':	// implemented as +? 

		// this case does not require the (y|) trick, noKLUDGE 

		INSERT(OPLUS_, pos);
		ASTERN(O_PLUS, pos);
		INSERT(OQUEST_, pos);
		ASTERN(O_QUEST, pos);
		break;
	case '+':
		INSERT(OPLUS_, pos);
		ASTERN(O_PLUS, pos);
		break;
	case '?':
		// KLUDGE: emit y? as (y|) until subtle bug gets fixed 

		INSERT(OCH_, pos);		// offset slightly wrong 

		ASTERN(OOR1, pos);		// this one's right 

		AHEAD(pos);			// fix the OCH_ 

		EMIT(OOR2, 0);			// offset very wrong... 

		AHEAD(THERE());			// ...so fix it 

		ASTERN(O_CH, THERETHERE());
		break;
	case '{':
		count = p_count(p);
		if (EAT(',')) {
			if (isdigit(PEEK())) {
				count2 = p_count(p);
				REQUIRE(count <= count2, REG_BADBR);
			} else		// single number with comma 

				count2 = INFINITY;
		} else		// just a single number 

			count2 = count;
		repeat(p, pos, count, count2);
		if (!EAT('}')) {	// error heuristics 

			while (MORE() && PEEK() != '}')
				NEXT();
			REQUIRE(MORE(), REG_EBRACE);
			SETERROR(REG_BADBR);
		}
		break;
	}

	if (!MORE())
		return;
	c = PEEK();
	if (!( c == '*' || c == '+' || c == '?' ||
				(c == '{' && MORE2() && isdigit(PEEK2())) ) )
		return;
	SETERROR(REG_BADRPT);
}

/*
 - p_count - parse a repetition count
 == static int p_count(register struct parse *p);
 */
static int			// the value 

p_count(register struct parse *p)
{
	register int count = 0;
	register int ndigits = 0;

	while (MORE() && isdigit(PEEK()) && count <= DUPMAX) {
		count = count*10 + (GETNEXT() - '0');
		ndigits++;
	}

	REQUIRE(ndigits > 0 && count <= DUPMAX, REG_BADBR);
	return(count);
}

/*
 - p_bracket - parse a bracketed character list
 == static void p_bracket(register struct parse *p);
 *
 * Note a significant property of this code:  if the allocset() did SETERROR,
 * no set operations are done.
 */
static void
p_bracket(register struct parse *p)
{
//	register char c;
	register cset *cs = allocset(p);
	register int invert = 0;

	// Dept of Truly Sickening Special-Case Kludges 

//	if (p->next + 5 < p->end && strncmp(p->next, "[:<:]]", 6) == 0) {
	if (p->end - p->next > 5 && strncmp(p->next, "[:<:]]", 6) == 0) {
		EMIT(OBOW, 0);
		NEXTn(6);
		return;
	}
//	if (p->next + 5 < p->end && strncmp(p->next, "[:>:]]", 6) == 0) {
	if (p->end - p->next > 5 && strncmp(p->next, "[:>:]]", 6) == 0) {
		EMIT(OEOW, 0);
		NEXTn(6);
		return;
	}

	if (EAT('^'))
		invert++;	// make note to invert set at end 

	if (EAT(']'))
		CHadd(cs, ']');
	else if (EAT('-'))
		CHadd(cs, '-');
	while (MORE() && PEEK() != ']' && !SEETWO('-', ']'))
		p_b_term(p, cs);
	if (EAT('-'))
		CHadd(cs, '-');
	MUSTEAT(']', REG_EBRACK);

	if (p->error != 0)	// don't mess things up further 

		return;

	if (p->g->cflags&REG_ICASE) {
		register int i;
		register int ci;

		for (i = p->g->csetsize - 1; i >= 0; i--)
			if (CHIN(cs, i) && isalpha(i)) {
				ci = othercase(i);
				if (ci != i)
					CHadd(cs, ci);
			}
		if (cs->multis)
			mccase(p, cs);
	}
	if (invert) {
		register int i;

		for (i = p->g->csetsize - 1; i >= 0; i--)
			if (CHIN(cs, i))
				CHsub(cs, i);
			else
				CHadd(cs, i);
		if (p->g->cflags&REG_SINGLELINE)
			CHsub(cs, '\n');
		if (cs->multis)
			mcinvert(p, cs);
	}

	ASSERT(!cs->multis);		// xxx 


	if (nch(p, cs) == 1) {		// optimize singleton sets 

		ordinary(p, firstch(p, cs));
		freeset(p, cs);
	} else
		EMIT(OANYOF, freezeset(p, cs));
}

/*
 - p_b_term - parse one term of a bracketed character list
 == static void p_b_term(register struct parse *p, register cset *cs);
 */
static void
p_b_term(register struct parse *p, register cset *cs)
{
	register char c;
	register char start, finish;
	register int i;

	// classify what we've got 

	switch ((MORE()) ? PEEK() : '\0') {
	case '[':
		c = (MORE2()) ? PEEK2() : '\0';
		break;
	case '-':
		SETERROR(REG_ERANGE);
		return;			// NOTE RETURN 
	default:
		c = '\0';
		break;
	}

	switch (c) {
	case ':':		// character class 

		NEXT2();
		REQUIRE(MORE(), REG_EBRACK);
		c = PEEK();
		REQUIRE(c != '-' && c != ']', REG_ECTYPE);
		p_b_cclass(p, cs);
		REQUIRE(MORE(), REG_EBRACK);
		REQUIRE(EATTWO(':', ']'), REG_ECTYPE);
		break;
	case '=':		// equivalence class 

		NEXT2();
		REQUIRE(MORE(), REG_EBRACK);
		c = PEEK();
		REQUIRE(c != '-' && c != ']', REG_ECOLLATE);
		p_b_eclass(p, cs);
		REQUIRE(MORE(), REG_EBRACK);
		REQUIRE(EATTWO('=', ']'), REG_ECOLLATE);
		break;
	default:		// symbol, ordinary character, or range 

// xxx revision needed for multichar stuff 

		start = p_b_symbol(p);
		if (SEE('-') && MORE2() && PEEK2() != ']') {
			// range 

			NEXT();
			if (EAT('-'))
				finish = '-';
			else
				finish = p_b_symbol(p);
		} else
			finish = start;
// xxx what about signed chars here... 

		REQUIRE(start <= finish, REG_ERANGE);
		for (i = start; i <= finish; i++)
			CHadd(cs, i);
		break;
	}
}

/*
 - p_b_cclass - parse a character-class name and deal with it
 == static void p_b_cclass(register struct parse *p, register cset *cs);
 */
static void
p_b_cclass(register struct parse *p, register cset *cs)
{
	register char *sp = p->next;
	register struct cclass *cp;
	register size_t len;
	register char *u;
	register char c;

	while (MORE() && isalpha(PEEK()))
		NEXT();
	len = p->next - sp;
	for (cp = cclasses; cp->name ; cp++)
		if (strncmp(cp->name, sp, len) == 0 && cp->name[len] == '\0')
			break;
	if (!cp->name) {
		// oops, didn't find it 

		SETERROR(REG_ECTYPE);
		return;
	}

	u = cp->chars;
	while ((c = *u++) != '\0')
		CHadd(cs, c);
	for (u = cp->multis; *u != '\0'; u += strlen(u) + 1)
		MCadd(p, cs, u);
}

/*
 - p_b_eclass - parse an equivalence-class name and deal with it
 == static void p_b_eclass(register struct parse *p, register cset *cs);
 *
 * This implementation is incomplete. xxx
 */
static void p_b_eclass (register struct parse *p, register cset *cs)
{
	register char c;

	c = p_b_coll_elem(p, '=');
	CHadd(cs, c);
}

/*
 - p_b_symbol - parse a character or [..]ed multicharacter collating symbol
 == static char p_b_symbol(register struct parse *p);
 */
static char			// value of symbol 

p_b_symbol(register struct parse *p)
{
	register char value;

	REQUIRE(MORE(), REG_EBRACK);
	if (!EATTWO('[', '.'))
		return(GETNEXT());

	// collating symbol 

	value = p_b_coll_elem(p, '.');
	REQUIRE(EATTWO('.', ']'), REG_ECOLLATE);
	return(value);
}

/*
 - p_b_coll_elem - parse a collating-element name and look it up
 == static char p_b_coll_elem(register struct parse *p, int endc);
 */
static char			// value of collating element 

p_b_coll_elem(register struct parse *p, int endc)	// name ended by endc,']' 

{
	register char *sp = p->next;
	register struct cname *cp;
	register int len;
//	register char c;

	while (MORE() && !SEETWO(endc, ']'))
		NEXT();
	if (!MORE()) {
		SETERROR(REG_EBRACK);
		return(0);
	}
	len = p->next - sp;
	for (cp = cnames; cp->name ; cp++)
		if (strncmp(cp->name, sp, len) == 0 && cp->name[len] == '\0')
			return(cp->code);	// known name 

	if (len == 1)
		return(*sp);	// single character 

	SETERROR(REG_ECOLLATE);			// neither 

	return(0);
}

/*
 - othercase - return the case counterpart of an alphabetic
 == static char othercase(int ch);
 */
static char			// if no counterpart, return ch 

othercase(int ch)
{
	ASSERT(isalpha(ch));
	if (isupper(ch))
		return ((char)tolower(ch));
	else if (islower(ch))
		return ((char)toupper(ch));
	else			// peculiar, but could happen

		return (char)(ch);
}

/*
 - bothcases - emit a dualcase version of a two-case character
 == static void bothcases(register struct parse *p, int ch);
 *
 * Boy, is this implementation ever a kludge...
 */
static void
bothcases(register struct parse *p, int ch)
{
	register char *oldnext = p->next;
	register char *oldend = p->end;
	char bracket[3];

	ASSERT(othercase(ch) != ch);	// p_bracket() would recurse 

	p->next = bracket;
	p->end = bracket+2;
	bracket[0] = (char)ch;
	bracket[1] = ']';
	bracket[2] = '\0';
	p_bracket(p);
	ASSERT(p->next == bracket+2);
	p->next = oldnext;
	p->end = oldend;
}

/*
 - ordinary - emit an ordinary character
 == static void ordinary(register struct parse *p, register int ch);
 */
static void ordinary (register struct parse *p, register int ch)
{
	register cat_t *cap = p->g->categories;

	if ((p->g->cflags&REG_ICASE) && isalpha(ch) && othercase(ch) != ch)
		bothcases(p, ch);
	else {
		EMIT(OCHAR, (unsigned char)ch);
		if (cap[ch] == 0)
			cap[ch] = (uch)p->g->ncategories++;
	}
}

/*
 - nonnewline - emit REG_SINGLELINE version of OANY
 == static void nonnewline(register struct parse *p);
 *
 * Boy, is this implementation ever a kludge...
 */
static void nonnewline(register struct parse *p)
{
	register char *oldnext = p->next;
	register char *oldend = p->end;
	char bracket[4];

	p->next = bracket;
	p->end = bracket+3;
	bracket[0] = '^';
	bracket[1] = '\n';
	bracket[2] = ']';
	bracket[3] = '\0';
	p_bracket(p);
	ASSERT(p->next == bracket+3);
	p->next = oldnext;
	p->end = oldend;
}

/*
 - repeat - generate code for a bounded repetition, recursively if needed
 == static void repeat(register struct parse *p, sopno start, int from, int to);
 */
static void
repeat(register struct parse *p, sopno start, int from, int to)
	// operand from here to end of strip 

	// repeated from this number 

	// to this number of times (maybe INFINITY) 

{
	register sopno finish = HERE();
#	define	N	2
#	define	INF	3
#	define	REP(f, t)	((f)*8 + (t))
#	define	MAP(n)	(((n) <= 1) ? (n) : ((n) == INFINITY) ? INF : N)
	register sopno copy;

	if (p->error != 0)	// head off possible runaway recursion 

		return;

	ASSERT(from <= to);

	switch (REP(MAP(from), MAP(to))) {
	case REP(0, 0):			// must be user doing this 

		DROP(finish-start);	// drop the operand 

		break;
	case REP(0, 1):			// as x{1,1}? 

	case REP(0, N):			// as x{1,n}? 

	case REP(0, INF):		// as x{1,}? 

		// KLUDGE: emit y? as (y|) until subtle bug gets fixed 

		INSERT(OCH_, start);		// offset is wrong... 

		repeat(p, start+1, 1, to);
		ASTERN(OOR1, start);
		AHEAD(start);			// ... fix it 

		EMIT(OOR2, 0);
		AHEAD(THERE());
		ASTERN(O_CH, THERETHERE());
		break;
	case REP(1, 1):			// trivial case 

		// done 

		break;
	case REP(1, N):			// as x?x{1,n-1} 

		// KLUDGE: emit y? as (y|) until subtle bug gets fixed 

		INSERT(OCH_, start);
		ASTERN(OOR1, start);
		AHEAD(start);
		EMIT(OOR2, 0);			// offset very wrong... 

		AHEAD(THERE());			// ...so fix it 

		ASTERN(O_CH, THERETHERE());
		copy = dupl(p, start+1, finish+1);
		ASSERT(copy == finish+4);
		repeat(p, copy, 1, to-1);
		break;
	case REP(1, INF):		// as x+ 

		INSERT(OPLUS_, start);
		ASTERN(O_PLUS, start);
		break;
	case REP(N, N):			// as xx{m-1,n-1} 

		copy = dupl(p, start, finish);
		repeat(p, copy, from-1, to-1);
		break;
	case REP(N, INF):		// as xx{n-1,INF} 

		copy = dupl(p, start, finish);
		repeat(p, copy, from-1, to);
		break;
	default:			// "can't happen" 

		SETERROR(REG_ASSERT);	// just in case 

		break;
	}
}

/*
 - seterr - set an error condition
 == static int seterr(register struct parse *p, int e);
 */
static int			// useless but makes type checking happy 

seterr(register struct parse *p, int e)
{
	if (p->error == 0)	// keep earliest error condition 

		p->error = e;
	p->next = nuls;		// try to bring things to a halt 

	p->end = nuls;
	return(0);		// make the return value well-defined 

}

/*
 - allocset - allocate a set of characters for []
 == static cset *allocset(register struct parse *p);
 */
static cset *
allocset(register struct parse *p)
{
	register int no = p->g->ncsets++;
	register size_t nc;
	register size_t nbytes;
	register cset *cs;
	register size_t css = (size_t)p->g->csetsize;
	register int i;

	if (no >= p->ncsalloc) {	// need another column of space 

		p->ncsalloc += CHAR_BIT;
		nc = p->ncsalloc;
		ASSERT(nc % CHAR_BIT == 0);
		nbytes = nc / CHAR_BIT * css;
		if (!p->g->sets)
			p->g->sets = (cset *)malloc(nc * sizeof(cset));
		else
			p->g->sets = (cset *)realloc((char *)p->g->sets,
							nc * sizeof(cset));
		if (!p->g->setbits)
			p->g->setbits = (uch *)malloc(nbytes);
		else {
			p->g->setbits = (uch *)realloc((char *)p->g->setbits,
								nbytes);
			// xxx this isn't right if setbits is now NULL 

			for (i = 0; i < no; i++)
				p->g->sets[i].ptr = p->g->setbits + css*(i/CHAR_BIT);
		}
		if (p->g->sets && p->g->setbits)
			(void) memset((char *)p->g->setbits + (nbytes - css),
								0, css);
		else {
			no = 0;
			SETERROR(REG_ESPACE);
			// caller's responsibility not to do set ops 

		}
	}

	ASSERT(p->g->sets);	// xxx 

	cs = &p->g->sets[no];
	cs->ptr = p->g->setbits + css*((no)/CHAR_BIT);
	cs->mask = (uch)(1 << ((no) % CHAR_BIT));
	cs->hash = 0;
	cs->smultis = 0;
	cs->multis = NULL;

	return(cs);
}

/*
 - freeset - free a now-unused set
 == static void freeset(register struct parse *p, register cset *cs);
 */
static void
freeset(register struct parse *p, register cset *cs)
{
	register int i;
	register cset *top = &p->g->sets[p->g->ncsets];
	register size_t css = (size_t)p->g->csetsize;

	for (i = 0; i < (int)css; i++)
		CHsub(cs, i);
	if (cs == top-1)	// recover only the easy case 

		p->g->ncsets--;
}

/*
 - freezeset - final processing on a set of characters
 == static int freezeset(register struct parse *p, register cset *cs);
 *
 * The main task here is merging identical sets.  This is usually a waste
 * of time (although the hash code minimizes the overhead), but can win
 * big if REG_ICASE is being used.  REG_ICASE, by the way, is why the hash
 * is done using addition rather than xor -- all ASCII [aA] sets xor to
 * the same value!
 */
static int			// set number 

freezeset(register struct parse *p, register cset *cs)
{
	register uch h = cs->hash;
	register int i;
	register cset *top = &p->g->sets[p->g->ncsets];
	register cset *cs2;
	register size_t css = (size_t)p->g->csetsize;

	// look for an earlier one which is the same 

	for (cs2 = &p->g->sets[0]; cs2 < top; cs2++)
		if (cs2->hash == h && cs2 != cs) {
			// maybe 

			for (i = 0; i < (int)css; i++)
				if (!!CHIN(cs2, i) != !!CHIN(cs, i))
					break;		// no 

			if (i == (int)css)
				break;			// yes 

		}

	if (cs2 < top) {	// found one 

		freeset(p, cs);
		cs = cs2;
	}

	return((int)(cs - p->g->sets));
}

/*
 - firstch - return first character in a set (which must have at least one)
 == static int firstch(register struct parse *p, register cset *cs);
 */
static int			// character; there is no "none" value 

firstch(register struct parse *p, register cset *cs)
{
	register int i;
	register size_t css = (size_t)p->g->csetsize;

	for (i = 0; i < (int)css; i++)
		if (CHIN(cs, i))
			return((char)i);
	ASSERT(false);
	return(0);		// arbitrary
}

/*
 - nch - number of characters in a set
 == static int nch(register struct parse *p, register cset *cs);
 */
static int
nch(register struct parse *p, register cset *cs)
{
	register int i;
	register size_t css = (size_t)p->g->csetsize;
	register int n = 0;

	for (i = 0; i < (int)css; i++)
		if (CHIN(cs, i))
			n++;
	return(n);
}

/*
 - mcadd - add a collating element to a cset
 == static void mcadd(register struct parse *p, register cset *cs, \
 ==	register char *cp);
 */
static void
mcadd(register struct parse *p, register cset *cs, register char *cp)
{
	register size_t oldend = cs->smultis;

	cs->smultis += strlen(cp) + 1;
	if (!cs->multis)
		cs->multis = (char *)malloc(cs->smultis);
	else
		cs->multis = (char *)realloc(cs->multis, cs->smultis);
	if (!cs->multis) {
		SETERROR(REG_ESPACE);
		return;
	}

	(void) strcpy(cs->multis + oldend - 1, cp);
	cs->multis[cs->smultis - 1] = '\0';
}

/*
 - mcsub - subtract a collating element from a cset
 == static void mcsub(register cset *cs, register char *cp);
 */
/* unused 
static void mcsub(register cset *cs, register char *cp)
{
	register char *fp = mcfind(cs, cp);
	register size_t len = strlen(fp);

	ASSERT(fp);
	(void) memmove(fp, fp + len + 1,
				cs->smultis - (fp + len + 1 - cs->multis));
	cs->smultis -= len;

	if (cs->smultis == 0) {
		free(cs->multis);
		cs->multis = NULL;
		return;
	}

	cs->multis = (char *)realloc(cs->multis, cs->smultis);
	ASSERT(cs->multis);
}
*/

/*
 - mcin - is a collating element in a cset?
 == static int mcin(register cset *cs, register char *cp);
 */
/* unusued
static int mcin(register cset *cs, register char *cp)
{
	return(mcfind(cs, cp) != NULL);
}
*/

/*
 - mcfind - find a collating element in a cset
 == static char *mcfind(register cset *cs, register char *cp);
 */
/* unused
static char *mcfind(register cset *cs, register char *cp)
{
	register char *p;

	if (!cs->multis)
		return(NULL);
	for (p = cs->multis; *p != '\0'; p += strlen(p) + 1)
		if (strcmp(cp, p) == 0)
			return(p);
	return(NULL);
}
*/

/*
 - mcinvert - invert the list of collating elements in a cset
 == static void mcinvert(register struct parse *p, register cset *cs);
 *
 * This would have to know the set of possibilities.  Implementation
 * is deferred.
 */
static void
mcinvert(register struct parse *, register cset *cs)
{	cs; // used!
  ASSERT(!cs->multis);	// xxx 
}

/*
 - mccase - add case counterparts of the list of collating elements in a cset
 == static void mccase(register struct parse *p, register cset *cs);
 *
 * This would have to know the set of possibilities.  Implementation
 * is deferred.
 */
static void
mccase(register struct parse *, register cset *cs)
{ cs; // used
	ASSERT(!cs->multis);	// xxx 

}

/*
 - isinsets - is this character in any sets?
 == static int isinsets(register struct re_guts *g, int c);
 */
static int			// predicate 

isinsets(register struct re_guts *g, int c)
{
	register uch *col;
	register int i;
	register int ncols = (g->ncsets+(CHAR_BIT-1)) / CHAR_BIT;
	register unsigned uc = (unsigned char)c;

	for (i = 0, col = g->setbits; i < ncols; i++, col += g->csetsize)
		if (col[uc] != 0)
			return(1);
	return(0);
}

/*
 - samesets - are these two characters in exactly the same sets?
 == static int samesets(register struct re_guts *g, int c1, int c2);
 */
static int			// predicate 

samesets(register struct re_guts *g, int c1, int c2)
{
	register uch *col;
	register int i;
	register int ncols = (g->ncsets+(CHAR_BIT-1)) / CHAR_BIT;
	register unsigned uc1 = (unsigned char)c1;
	register unsigned uc2 = (unsigned char)c2;

	for (i = 0, col = g->setbits; i < ncols; i++, col += g->csetsize)
		if (col[uc1] != col[uc2])
			return(0);
	return(1);
}

/*
 - categorize - sort out character categories
 == static void categorize(struct parse *p, register struct re_guts *g);
 */
static void
categorize(struct parse *p, register struct re_guts *g)
{
	register cat_t *cats = g->categories;
	register int c;
	register int c2;
	register cat_t cat;

	// avoid making error situations worse 

	if (p->error != 0)
		return;

	for (c = CHAR_MIN; c <= CHAR_MAX; c++)
		if (cats[c] == 0 && isinsets(g, c)) {
			cat = (cat_t)g->ncategories++;
			cats[c] = cat;
			for (c2 = c+1; c2 <= CHAR_MAX; c2++)
				if (cats[c2] == 0 && samesets(g, c, c2))
					cats[c2] = cat;
		}
}

/*
 - dupl - emit a duplicate of a bunch of sops
 == static sopno dupl(register struct parse *p, sopno start, sopno finish);
 */
static sopno			// start of duplicate 

dupl(register struct parse *p, sopno start, sopno finish)
	// from here 

	// to this less one 

{
	register sopno ret = HERE();
	register sopno len = finish - start;

	ASSERT(finish >= start);
	if (len == 0)
		return(ret);
	enlarge(p, p->ssize + len);	// this many unexpected additions 

	ASSERT(p->ssize >= p->slen + len);
	(void) memcpy((char *)(p->strip + p->slen),
		(char *)(p->strip + start), (size_t)len*sizeof(sop));
	p->slen += len;
	return(ret);
}

/*
 - doemit - emit a strip operator
 == static void doemit(register struct parse *p, sop op, size_t opnd);
 *
 * It might seem better to implement this as a macro with a function as
 * hard-case backup, but it's just too big and messy unless there are
 * some changes to the data structures.  Maybe later.
 */
static void
doemit(register struct parse *p, sop op, size_t opnd)
{
	// avoid making error situations worse 

	if (p->error != 0)
		return;

	// deal with oversize operands ("can't happen", more or less) 

	ASSERT(opnd < (1<<OPSHIFT));

	// deal with undersized strip 

	if (p->slen >= p->ssize)
		enlarge(p, (p->ssize+1) / 2 * 3);	// +50% 

	ASSERT(p->slen < p->ssize);

	// finally, it's all reduced to the easy case 

	p->strip[p->slen++] = SOP(op, opnd);
}

/*
 - doinsert - insert a sop into the strip
 == static void doinsert(register struct parse *p, sop op, size_t opnd, sopno pos);
 */
static void
doinsert(register struct parse *p, sop op, size_t opnd, sopno pos)
{
	register sopno sn;
	register sop s;
	register int i;

	// avoid making error situations worse 

	if (p->error != 0)
		return;

	sn = HERE();
	EMIT(op, opnd);		// do checks, ensure space 

	ASSERT(HERE() == sn+1);
	s = p->strip[sn];

	// adjust paren pointers 

	ASSERT(pos > 0);
	for (i = 1; i < NPAREN; i++) {
		if (p->pbegin[i] >= pos) {
			p->pbegin[i]++;
		}
		if (p->pend[i] >= pos) {
			p->pend[i]++;
		}
	}

	memmove((char *)&p->strip[pos+1], (char *)&p->strip[pos],
						(HERE()-pos-1)*sizeof(sop));
	p->strip[pos] = s;
}

/*
 - dofwd - complete a forward reference
 == static void dofwd(register struct parse *p, sopno pos, sop value);
 */
static void
dofwd(register struct parse *p, register sopno pos, sop value)
{
	// avoid making error situations worse 

	if (p->error != 0)
		return;

	ASSERT(value < (1<<OPSHIFT));
	p->strip[pos] = OP(p->strip[pos]) | value;
}

/*
 - enlarge - enlarge the strip
 == static void enlarge(register struct parse *p, sopno size);
 */
static void
enlarge(register struct parse *p, register sopno size)
{
	register sop *sp;

	if (p->ssize >= size)
		return;

	sp = (sop *)realloc(p->strip, size*sizeof(sop));
	if (!sp) {
		SETERROR(REG_ESPACE);
		return;
	}
	p->strip = sp;
	p->ssize = size;
}

/*
 - stripsnug - compact the strip
 == static void stripsnug(register struct parse *p, register struct re_guts *g);
 */
static void
stripsnug(register struct parse *p, register struct re_guts *g)
{
	g->nstates = p->slen;
	g->strip = (sop *)realloc((char *)p->strip, p->slen * sizeof(sop));
	if (!g->strip) {
		SETERROR(REG_ESPACE);
		g->strip = p->strip;
	}
}

/*
 - findmust - fill in must and mlen with longest mandatory literal string
 == static void findmust(register struct parse *p, register struct re_guts *g);
 *
 * This algorithm could do fancy things like analyzing the operands of |
 * for common subsequences.  Someday.  This code is simple and finds most
 * of the interesting cases.
 *
 * Note that must and mlen got initialized during setup.
 */
static void
findmust(struct parse *p, register struct re_guts *g)
{
	register sop *scan;
	sop *start=0;
	register sop *newstart=0;
	register sopno newlen;
	register sop s;
	register char *cp;
	register sopno i;

	// avoid making error situations worse 

	if (p->error != 0)
		return;

	// find the longest OCHAR sequence in strip 

	newlen = 0;
	scan = g->strip + 1;
	do {
		s = *scan++;
		switch (OP(s)) {
		case OCHAR:		// sequence member 

			if (newlen == 0)		// new sequence 

				newstart = scan - 1;
			newlen++;
			break;
		case OPLUS_:		// things that don't break one 

		case OLPAREN:
		case ORPAREN:
			break;
		case OQUEST_:		// things that must be skipped 

		case OCH_:
			scan--;
			do {
				scan += OPND(s);
				s = *scan;
				// ASSERT() interferes w debug printouts 

				if (OP(s) != O_QUEST && OP(s) != O_CH &&
							OP(s) != OOR2) {
					g->iflags |= BAD;
					return;
				}
			} while (OP(s) != O_QUEST && OP(s) != O_CH);
			// fallthrough 

		default:		// things that break a sequence 

			if (newlen > g->mlen) {		// ends one 

				start = newstart;
				g->mlen = newlen;
			}
			newlen = 0;
			break;
		}
	} while (OP(s) != OEND);

	if (g->mlen == 0)		// there isn't one 

		return;

	// turn it into a character string 

	g->must = (char *)malloc((size_t)g->mlen + 1);
	if (!g->must) {		// argh; just forget it 

		g->mlen = 0;
		return;
	}
	cp = g->must;
	scan = start;
	for (i = g->mlen; i > 0; i--) {
		while (OP(s = *scan++) != OCHAR)
			continue;
		ASSERT(cp < g->must + g->mlen);
		*cp++ = (char)OPND(s);
	}
	ASSERT(cp == g->must + g->mlen);
	*cp = '\0';		// just on general principles 

}

/*
 - pluscount - count + nesting
 == static sopno pluscount(register struct parse *p, register struct re_guts *g);
 */
static sopno			// nesting depth 

pluscount(struct parse *p, register struct re_guts *g)
{
	register sop *scan;
	register sop s;
	register sopno plusnest = 0;
	register sopno maxnest = 0;

	if (p->error != 0)
		return(0);	// there may not be an OEND 


	scan = g->strip + 1;
	do {
		s = *scan++;
		switch (OP(s)) {
		case OPLUS_:
			plusnest++;
			break;
		case O_PLUS:
			if (plusnest > maxnest)
				maxnest = plusnest;
			plusnest--;
			break;
		}
	} while (OP(s) != OEND);
	if (plusnest != 0)
		g->iflags |= BAD;
	return(maxnest);
}


////////////////////////////////////////////////////////////////////////////////
//	R E G F R E E . C

// regfree - free everything
void regfree(regex_it *preg)
{
	struct re_guts *g;

	// Check for first signature
	if (preg->re_magic != MAGIC1)
		return;

	// Check for second signature
	g = preg->re_g;
	if (!g || g->magic != MAGIC2)
		return;

	// Mark the structure as invalid
	preg->re_magic = 0;
	g->magic = 0;

	// Free things we still have allocated
	if (g->strip)	free(g->strip);
	if (g->sets)	free(g->sets);
	if (g->setbits)	free(g->setbits);
	if (g->must)	free(g->must);
	free(g);
}


////////////////////////////////////////////////////////////////////////////////
//	R E G E R R O R . C

static char *regatoi (const regex_it *preg, char *localbuf);

/*
 = #define	REG_NOMATCH	 1
 = #define	REG_BADPAT	 2
 = #define	REG_ECOLLATE	 3
 = #define	REG_ECTYPE	 4
 = #define	REG_EESCAPE	 5
 = #define	REG_ESUBREG	 6
 = #define	REG_EBRACK	 7
 = #define	REG_EPAREN	 8
 = #define	REG_EBRACE	 9
 = #define	REG_BADBR	10
 = #define	REG_ERANGE	11
 = #define	REG_ESPACE	12
 = #define	REG_BADRPT	13
 = #define	REG_EMPTY	14
 = #define	REG_ASSERT	15
 = #define	REG_INVARG	16
 = #define	REG_ATOI	255	// convert name to number (!)
 = #define	REG_ITOA	0400	// convert number to name (!)
 */
static struct rerr {
	int code;
	char *name;
	char *explain;
}
rerrs[] =
{
       {REG_NOMATCH,	"REG_NOMATCH",	"regular expression not matched"},
       {REG_BADPAT,		"REG_BADPAT",	"invalid regular expression"},
       {REG_ECOLLATE,	"REG_ECOLLATE",	"invalid collating element"},
       {REG_ECTYPE,		"REG_ECTYPE",	"invalid character class"},
       {REG_EESCAPE,	"REG_EESCAPE",	"invalid escape \\"},
       {REG_ESUBREG,	"REG_ESUBREG",	"invalid backreference value"},
       {REG_EBRACK,		"REG_EBRACK",	"unbalanced square brackets []"},
       {REG_EPAREN,		"REG_EPAREN",	"unbalanced parentheses"},
       {REG_EBRACE,		"REG_EBRACE",	"unbalanced braces"},
       {REG_BADBR,		"REG_BADBR",	"invalid repetition count"},
       {REG_ERANGE,		"REG_ERANGE",	"invalid character range"},
       {REG_ESPACE,		"REG_ESPACE",	"out of memory"},
       {REG_BADRPT,		"REG_BADRPT",	"invalid repetition operand"},
       {REG_EMPTY,		"REG_EMPTY",	"empty (sub)expression"},
       {REG_ASSERT,		"REG_ASSERT",	"BUG in regex code"},
       {REG_INVARG,		"REG_INVARG",	"invalid argument"},
       {0,				"",				"*** unknown regexp error code ***"}
};

/*
 - regerror - the interface to error numbers
 = extern size_t regerror(int, const regex_it *, char *, size_t);
 */
size_t regerror (int errcode, const regex_it *preg, char *errbuf, size_t errbuf_size)
{
	register struct rerr *r;
	register size_t len;
	register int target = errcode &~ REG_ITOA;
	register char *s;
	char convbuf[50];

	if (errcode == REG_ATOI)
		s = regatoi(preg, convbuf);
	else
	{
		for (r = rerrs; r->code != 0; r++)
		{
			if (r->code == target)
				break;
		}

		if (errcode&REG_ITOA)
		{
			if (r->code) strcpy(convbuf, r->name);
			else {strcpy(convbuf,"REG_"); _itoa(target,convbuf+4,10);}
			ASSERT(strlen(convbuf) < sizeof(convbuf));
			s = convbuf;
		}
		else
			s = r->explain;
	}

	len = strlen(s) + 1;
	if (errbuf_size > 0)
	{
		if (errbuf_size > len)
			(void) strcpy(errbuf, s);
		else
		{
			(void) strncpy(errbuf, s, errbuf_size-1);
			errbuf[errbuf_size-1] = '\0';
		}
	}

	return(len);
}

/*
 - regatoi - internal routine to implement REG_ATOI
 == static char *regatoi(const regex_it *preg, char *localbuf);
 */
static char *regatoi (const regex_it *preg, char *localbuf)
{ register struct rerr *r;
  for (r = rerrs; r->code ; r++)
	{ if (!strcmp(r->name, preg->re_endp))
		break;
	}
	if (!r->code) return("0");
  //
  _itoa(r->code,localbuf,10);
  return(localbuf);
}



// -----------

bool regbegin(REGEXP *reg, const char *pattern, int cflags)
{ ASSERT(sizeof(regex_it)<=sizeof(REGEXP));
  regex_it *preg = (regex_it*)reg;
  int res = regcomp(preg, pattern, cflags);
  return (res==REG_SUCCESS);
}

bool regmatch(const REGEXP *reg, const char *str, REGMATCH *pmatch, int nmatch)
{ regex_it *preg = (regex_it*)reg;
  int res = regexec(preg,str,nmatch,pmatch,0);
  // the regexec code uses "start and end", but we convert it into "start and len"
  for (int i=0; i<nmatch; i++)
  { pmatch[i].len = pmatch[i].len-pmatch[i].i;
  }
  return (res==REG_SUCCESS);
}

void regend(REGEXP *reg)
{ regex_it *preg = (regex_it*)reg;
  regfree(preg);
}


