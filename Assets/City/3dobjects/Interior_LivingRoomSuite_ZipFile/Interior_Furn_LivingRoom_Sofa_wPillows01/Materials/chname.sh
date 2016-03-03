 for f in AutoSave*.mat
do
 echo "Processing $f"
 mv "$f" "${f:70}";
done
