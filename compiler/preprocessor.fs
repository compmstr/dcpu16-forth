needs ../utils/strings.fs
needs ../utils/util.fs
needs ../utils/files.fs
needs macro.fs

\ finds and stores macros in dasm file and #included files
: find-macros ( loc len -- )
;

\ preprocesses a dasm file, replacing includes with contents, and
\   macros with definitions
: preprocess-file ( loc len -- )
;
