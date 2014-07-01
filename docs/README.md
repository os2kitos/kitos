 How to doxygen
 ======
 The doxy.conf file have been set to only look in:
 ../Core.ApplicationServices/ ../Core.DomainModel/ ../Core.DomainServices/ ../Infrastructure.DataAccess/ ../UI.MVC4/
 
 So if anymore are added they should be added to the config as well.
 
 Generate
 ======
 Run this command from this folder:  doxygen.exe doxy.conf
 
 The generated doc will be put in a folder named output.
