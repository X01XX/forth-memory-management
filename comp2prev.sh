#!/usr/bin/bash
ls -dtr /media/earl/UDISK/gforth-mma2* | xargs -n 1 basename

/bin/echo "Enter gforth-mma2 or gforth-mma2_YYYY_.."
read GM2
/bin/echo "< previous, > current" > outcomp.txt
for f in ~/gforth-mma2/*.fs
do
	echo $f >> outcomp.txt
	diff /media/$USER/UDISK/$GM2/$(basename -- $f) $f >> outcomp.txt
done
cat outcomp.txt
