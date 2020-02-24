library(ggcorrplot)
library(caret)
library(e1071)
library(readxl)
library(ggplot2)
library(dplyr)


# Names
names = c("p1")
cond = c("Baseline1","Approach","Baseline2")
mode = c("main")


# 1. 1 Letter Accuracy [%]  

name_real = "p12_Approach_main.csv"

base_df = data.frame()
for (i in 1:1){
  file_name = "YB_B_main.csv"
  file_data = read.csv(file_name, header=T, stringsAsFactors = F)
  base_df = rbind(base_df,file_data)
}
base_df


x1 = ""
x2 = ""

for(i in 1:length(base_df$realPattern)){
  if(substr(toString(base_df$realPattern[i]), 1,1) == "1"){
    x1 = "2"
  }
  else if (substr(toString(base_df$realPattern[i]), 1,1) == "2"){
    x1 = "4"
  }
  else if (substr(toString(base_df$realPattern[i]), 1,1) == "3"){
    x1 = "1"
  }
  else if (substr(toString(base_df$realPattern[i]), 1,1) == "4"){
    x1 = "3"
  }
  
  if(substr(toString(base_df$realPattern[i]), 2,2) == "1"){
    x2 = "2"
  }
  else if (substr(toString(base_df$realPattern[i]), 2,2) == "2"){
    x2 = "4"
  }
  else if (substr(toString(base_df$realPattern[i]), 2,2) == "3"){
    x2 = "1"
  }
  else if (substr(toString(base_df$realPattern[i]), 2,2) == "4"){
    x2 = "3"
  }
  
  modified = strtoi(paste(x1,x2,sep=""))
  base_df$realPattern[i] = modified
}

base_df

for (j in 1:length(base_df$userAnswer)){
  if(base_df$realPattern[j]==base_df$userAnswer[j]){
    base_df$correct[j] = "1"
  }
  else{
    base_df$correct[j] = "0"
    
  }
  
  if( substr( toString(base_df$realPattern[j]), 1, 1 ) == substr( toString(base_df$userAnswer[j]),1,1) ){
    base_df$c1[j] = "1"
  }
  else{
    base_df$c1[j] = "0"
  }
  
  if( substr( toString(base_df$realPattern[j]), 2, 2 ) == substr( toString(base_df$userAnswer[j]),2,2) ){
    base_df$c2[j] = "1"
  }
  else{
    base_df$c2[j] = "0"
  }

}


base_df

write.csv(base_df,"YB_B_main_modified.csv",row.names=FALSE)
















for(i in 1:length(base_df$userAnswer)){
  if(base_df$realPattern[i] == base_df$userAnswer[i]){
    base_df$correct[i] = 1
  }
  else
  {
    base_df$correct[i] = 0
  }
}
base_df
if(base_df$realPattern[1] ==base_df$userAnswer[1] )
{
  print("Hi!")
  
}


toString(base_df$userAnswer[1])
substr(toString(base_df$userAnswer[1]), 1,1)
substr(toString(base_df$userAnswer[1]), 2,2)
substr(toString(base_df$userAnswer[1]), 3,3)
if(substr(toString(base_df$userAnswer[1]), 3,3) == "2")
{
  print("Hello")
  
}

a = "1"
b = "2"
c = "3"
a+b

x = paste(a,b,c,sep="")

p = strtoi(x)
str(p)

str(base_df$userAnswer[1])

# Confusion matrix
actual_first <- as.factor(base_df$realPattern)
predicted_first <- as.factor(base_df$userAnswer)

str(actual_first)
str(predicted_first)



length(actual_first)
length(predicted_first)


cm_first <- confusionMatrix(predicted_first, actual_first)
cm_temp = cm_first$table 

cm_df <- as.data.frame(as.matrix(cm_temp))
cm_df

ggplot(cm_df) +
  geom_tile(aes(x=Prediction, y=Reference, fill=Freq)) +
  coord_equal() +
  geom_text(aes(x=Prediction, y=Reference, label = sprintf("%1.0f", Freq)), vjust = 0.3, size= 4)+
  labs(title="", x="User Typed Pattern", y="Actual Pattern") +
  scale_y_discrete(limits = rev(levels(cm_df$Reference))) +
  scale_fill_gradient(low="white", high="slategrey") +
  theme_bw()



# Names
names = c("p1","p2","p3","p4","p5","p6","p7","p8","p9","p10","p11","p12")
cond = c("Baseline1","Approach","Baseline2")
mode = c("training","main")
# 1. 1 Letter Accuracy [%]  
base_df = data.frame()
for (i in 1:12){
  file_name = paste("Exp6-(2)_data/",names[i],"_",cond[1],"_",mode[2],".csv",sep="")
  file_data = read.csv(file_name, header=T, stringsAsFactors = F)
  base_df = rbind(base_df,file_data)
}
base_df


