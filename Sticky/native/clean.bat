if NOT EXIST clean.bat goto end

del /q *.~*
del /q *.obj

del /q wmp\Release\*
del /q wmp\Debug\*
rmdir wmp\Release
rmdir wmp\Debug
del wmp\wmp.ncb
del wmp\wmp.suo

del /q winamp3\Release\*
del /q winamp3\Debug\*
rmdir winamp3\Release
rmdir winamp3\Debug
del winamp3\sticky3.aps
del winamp3\sticky3.ncb
del winamp3\sticky3.suo

del /q autoparse\Release\*
del /q autoparse\Debug\*
rmdir autoparse\Release
rmdir autoparse\Debug
del autoparse\autoparse.aps
del autoparse\autoparse.ncb
del autoparse\autoparse.suo
del autoparse\autohtml.aps
del autoparse\autohtml.ncb
del autoparse\autohtml.suo
del autoparse\autozip.aps
del autoparse\autozip.ncb
del autoparse\autozip.suo

del /q winamp\Release\*
del /q winamp\Debug\*
rmdir winamp\Release
rmdir winamp\Debug
del winamp\sticky2.ncb
del winamp\sticky2.suo

del /q sticked\Release\*
del /q sticked\Debug\*
rmdir sticked\Release
rmdir sticked\Debug
del /q sticked\*.~*
del sticked\sticked.exe
del sticked\edres.aps
del sticked\sticked.cgl
del sticked\sticked.dsk
del sticked\sticked.obj
del sticked\zip.obj
del sticked\edres.res
del sticked\sticked.tds
del sticked\sticked.ncb
del sticked\sticked.suo

copy scr\Release\sfp.scr StickySaver_setup.exe
del /q scr\Debug\*
del /q scr\Release\*
rmdir scr\Debug
rmdir scr\Release
del scr\*.~*
del scr\sfp.aps
del scr\sfp.cgl
del scr\sfp.dsk
del scr\sfp.ncb
del scr\sfp.obj
del scr\sfp.res
del scr\sfp.scr
attrib -h scr\sfp.suo
del scr\sfp.suo
del scr\sfp.tds

copy ape\Release\sticky.ape sticky.ape
del /q ape\Release\*
del /q ape\Debug\*
rmdir ape\Release
rmdir ape\Debug
del ape\*.~*
del ape\stickyape.aps
del ape\stickyape.ncb
attrib -h ape\stickyape.suo
del ape\stickyape.suo

del /q install\disk1\*
rmdir install\disk1
del install\disk1.zip
del /q "install\media\New Media\disk images\disk1\*"
rmdir "install\media\New Media\disk images\disk1"
del /q "install\media\New Media\log files\*"
del /q "install\media\New Media\report files\*"
del install\install.ncb
del install\install.suo


:end
