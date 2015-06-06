#ifndef __regex_H
#define __regex_H

// structures used by the functions
typedef struct {int nsub; void *reserved[4];} REGEXP;
typedef struct {long i; long len;} REGMATCH;

// flags to be passed to regbegin
#define REG_ICASE       0002   // case-insensitive search
#define REG_SINGLELINE  0010   // prevents . from matching a \n

// the functions
bool regbegin(REGEXP *reg, const char *pattern, int flags);
bool regmatch(const REGEXP *reg, const char *str, REGMATCH *matches, int nmatches);
void regend(REGEXP *reg);

// e.g.
//   const char *src="file.ext";
//   REGEXP re; regbegin(&re,"(.*)(\\..*)",0);
//   REGMATCH m[3]; regmatch(&re, src,m,3);
//   regend(&re);
//   string fn (src+m[1].i, m[1].len);  // "file"
//   string ext(src+m[2].i, m[2].len);  // ".ext"

#endif
