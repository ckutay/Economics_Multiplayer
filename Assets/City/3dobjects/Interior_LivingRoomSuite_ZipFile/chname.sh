 for f in Proj*.mat
do
 echo "Processing $f"
 mv "$f" "${f:70}";
done
