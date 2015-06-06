cd gdbsd
gcc -I. -I/usr/local/include -c *.c
cd ..
g++ -Igdbsd -DZIP_STD -o cross cross.cpp body.cpp unzip.cpp gdbsd/*.o /usr/local/lib/libjpeg.so
