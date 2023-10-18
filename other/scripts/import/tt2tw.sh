#! /bin/bash

# Script to input TimeTagger (https://github.com/almarklein/time-tagger) records into Timewarrior (https://github.com/GothenburgBitFactory/timewarrior), via a provided textfile containing records exported from the app in ISO 8601 format.

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

# check first line has time
if [ "$(awk '/([0-9]+[-])+([0-9]+[T])+([0-9]+[:])+([0-9]+[Z]?)/ {print}' $file)" ]; then
    printf "File has ${grn}correct${noc} time format. " 
else
    printf "File has ${red}incorrect${noc} time format."
    exit
fi

# report to user if lines have tags
if [ $(awk '/#/ {print}' $file | wc -l) -lt $(awk '{print}' $file | wc -l) ]; then
    printf "Some records are ${red}missing tags${noc}. "
else
    printf "Every record is ${grn}tagged${noc}! "
fi

printf "\n"

# create temporary file for formatted lines from records
tmpfile=$(mktemp /tmp/tt2tw_recs.XXXXXXX)

# read all record lines, stopping to create tags-string where needed, and save to temp file
terminal=$(tty)
while read line
do
    oldrecord=($line)
    # if [[ "${oldrecord[@]:3}" =~ [#][a-zA-Z0-9]+ ]]; then

	tags=$(echo "$line" | awk 'BEGIN {FS="\t"}; {print $4}')
	description=$(echo "$line" | awk 'BEGIN {FS="\t"}; {print $5}' | sed 's/#//g' )

	if [[ $tags =~ [#] ]]; then
		checkedtags=$( echo $tags | sed 's/#//g')
    else
        echo "Enter space separated tags as a replacement for the description:"
		echo -e	"${yel}$description${noc}"
        read -p "Tags: " < $terminal
        checkedtags=$REPLY
    fi

    newrecord="${oldrecord[1]} to ${oldrecord[2]} $checkedtags"
    echo $newrecord >> $tmpfile

done < $file

# report a preview of first few new records before proceeding to input or exit
preview=$(head --lines=5 $tmpfile)
echo -e "${yel}$preview${noc}"
read -p "Continue? [y/N] "
case "$REPLY" in
	[yY][eE][sS]|[yY])
		input=1
		while read line
		do
			if [ $input == 1 ]; then
				iNputresponse=$(timew track $line)

				if [ "$inputresponse" == "You cannot overlap intervals. Correct the start/end time, or specify the :adjust hint." ]; then
					[ -z "$rejfile" ] && rejfile=$(mktemp /tmp/tt2tw_rejects.XXXXXXX)
					echo $line >> $rejfile
				fi

				echo "${inputresponse}"

				read -p "Input next record? [Y/N/[U]ndo]" < $terminal
				case "$REPLY" in
					[yY][eE][sS]|[yY])
						input=1	
						;;
					[uU][nN][dD][oO]|[uU])
						input=1
						echo $(timew undo)
						[ -z "$rejfile" ] && rejfile=$(mktemp /tmp/tt2tw_rejects.XXXXXXX)
						echo $line >> $rejfile
						;;
					[*])
						input=0
				esac
			else
				[ -z "$remainfile" ] && remainfile=$(mktemp /tmp/tt2tw_remaining.XXXXXXX)
				echo $line >> $remainfile
			fi
		done < $tmpfile
		
		if [ "$rejfile" ]; then
			cp $rejfile $PWD 
			echo -e "${yel}Reject (undo) records file${noc} copied to current directory." 
			rm "$rejfile"
		elif [ "$remainfile"]; then
			cp $remainfile $PWD
			echo -e "${red}Remaining records file${noc} copied to current directory." 
			rm "$remainfile"
		elif [ "$remainfile" && "$refile" ]; then
			echo -e "${grn}Completed perfectly.${noc}"
		fi
		
		rm "$remainfile"
		exit
		;;
	*)
		rm "$tmpfile"
		exit
		;;
esac
