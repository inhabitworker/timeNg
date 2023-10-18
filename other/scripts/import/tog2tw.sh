#! /bin/bash

# Script to input unflitered "detailed" Toggl Track (https://track.toggl.com/reports/detailed) records, downloaded as csv file, into Timewarrior (https://github.com/GothenburgBitFactory/timewarrior).

# The csv file should have format with fields:
# User,Email,Client,Project,Task,Description,Billable,Start date,Start time,End date,End time,Duration,Tags,Amount ()

# styling variables
noc="\033[0m"
gry="\033[0;37m"
yel="\033[0;33m"
grn="\033[0;32m"
red="\033[0;31m"

# get file path from argument or input
if [[ $1 != "" ]]; then
    filename=$1
else
    read -p "Name or path to file containing records: "
    filename=$REPLY
fi

# verify file
file=$PWD/$filename
echo -e "Checking file ${gry}$file${noc}..."
if [ -e $file ]; then
    echo -e "File ${grn}exists${noc}."
else
    echo -e "File ${red}does not exist${noc}."
    exit
fi

# check format: 
# any field content followed by comma, 13 times and no newlines, with a final field content ending with newline
# ([^,\n]*,){13}[^,]*\n or... sed -n -E '/([^,\n]*,){13}[^,\n]*/p'

if [ "$(sed -n -E '/([^,\n]*,){13,}[^,\n]*/p' $file)" ]; then
    echo -e "File format seems ${grn}correct${noc}."
else
    echo -e "File format seems ${red}incorrect${noc}."
	exit
fi

# create temporary file and tty stdin ref
tmpfile=$(mktemp /tmp/tog2tw_tmp.XXXXXXX)
terminal=$(tty)

# format and report a preview of first few formatted records 
while read line
do
	if [ "$( echo $line | awk '/User,Email,Client/ {print}')" ]; then
		echo -e "File ${yel}starts with fields description${noc}, skipping first line."
	else
		timedate=$(echo $line | awk 'BEGIN{FS=","} {print $8"T"$9" to "$10"T"$11}')
		tagscript='
			BEGIN {
				FS=","
			}

			{
				gsub(/, /,"\42 \42",$0)
				
				quote="\42"
				if ($3)
					tags[0] = quote $3 quote
				if ($4)
					tags[1] = quote $4 quote
				if ($5)
					tags[2] = quote $5 quote
				if ($6)
					tags[3] = quote $6 quote

				if ($13) {
					split($13,toggltags," ")
					i=4
					for (tag in toggltags) {
						tags[i] = toggltags[tag]
						i++
					}
				}
				
				for (tag in tags)
					print tags[tag]
			}
			' 
			tags=($( echo $line | awk "$tagscript" ))
			echo "$timedate ${tags[@]}" >> $tmpfile
	fi
done < $file

# input interaction
echo -e "${yel}Preview: ${noc}"
head $tmpfile
read -p "Continue? [y/N] "

case "$REPLY" in
	[yY][eE][sS]|[yY])
		while read line
		do
			inputresponse="timew track $(echo $line)"
			# not sure why I have to do this but it work
			# maybe it execute in mine own shell, for the record problem was strange escape double quote ruining my tags
			eval $inputresponse
			errors=0

			if [ $? -ne 0 ]; then
				[ -z "$rejfile" ] && rejfile=$(mktemp /tmp/tog2tw_rejects.XXXXXXX)
				echo $line >> $rejfile
				$errors++
			fi

		done < $tmpfile
		
		if [ "$rejfile" ]; then
			cp $rejfile $PWD 
			echo -e "Some records ${red}weren't input successfully${noc}, a file containing them has been copied to current directory." 
			rm "$rejfile"
			read -p "Do you want to undo all of the records? [y/N] " < $terminal
			case "$REPLY" in
				[yY][eE][sS]|[yY])
					recordcount=$(wc -l < $file)
					undocount=$(( $recordcount - $errors ))
					for i in {1..$undocount}
					do
						echo $(echo "timew undo")
					done
				;;
			esac	
		else 
			echo -e "${grn}Complete!${noc}"
		fi
		;;	

	*)
		echo "Removing temporary files and exiting..."
		rm "$tmpfile"
		exit
		;;
esac
