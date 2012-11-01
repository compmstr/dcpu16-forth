needs ../../utils/sdl.fs
needs ../../utils/files.fs

variable screen-cur-font
0 screen-cur-font !
variable screen-current-pallette
0 screen-cur-pallete !
variable screen-last-update
0 screen-last-update !
variable default-font
0 default-font !

: load-default-font
;

: screen-updater
;
: screen-hw-int-handler
;
: screen-get-hw-info
;

' screen-updater
cr dup ." Screen Updater: " . cr
' screen-hw-int-handler
dup ." Screen Int-handler: " . cr
' screen-get-hw-info
dup ." Screen Info: " . cr
