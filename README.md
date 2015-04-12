# SmallToys
>some small coding items
##PickPattern
<p>this branch includes some codes aim to pick up the repeated pattern from a string of data deemed to be periodical</p>
<p>the DataPreprocess.dll to be compiled will provide following method:</p>
+ PickPattern Method: pick up the pattern deemed to be directly from the string of data;
+ SorpToPattern Method: sorpt the string of data to the pattern for every period, method will create a shallow copy of original data so that will not affect it;
+ GeneratePattern Method: generate a pattern by iterating PickPattern method and SorpToPattern Method;there are two overloads, one iterates for given times, the other iterates till the method is converged;
+ GetPeriod Method: return length of period according to the picked-up pattern;
+ GetNumOfPeriod Method: return the repeating times of picked-up pattern;
+ engthAdaption Method: compress or strech the data to the particular length. it compresses by cubic spline interpolation;the triSpline.dll previded by jimodeduzou on CSDN.net
  
