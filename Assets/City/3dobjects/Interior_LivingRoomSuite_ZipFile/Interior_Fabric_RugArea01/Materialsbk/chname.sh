 for f in *.mat
do
 echo "Processing $f"
 mv "$f" "${f19:}";
done
