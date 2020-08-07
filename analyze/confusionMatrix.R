library(ggcorrplot)
library(caret)
library(e1071)
library(readxl)
library(ggplot2)
library(dplyr)


# Names
names = c("p2","p3","p4","p6","p7","p8","p9","p10","p11","p12")
cond = c("Distal","Body")
mode = c("training","main")
# 1. 1 Letter Accuracy [%]
base_df = data.frame()
for(j in 1:2)
{
  for (i in 1:10){
    file_name = paste("Exp7_data/",names[i],"_",cond[j],"_",mode[2],".csv",sep="")
    file_data = read.csv(file_name, header=T, stringsAsFactors = F)
    base_df = rbind(base_df,file_data)
  }  
}

base_df
base_df_armfront = base_df[which(base_df$cond == "Distal"),]

base_df_armbody = base_df[which(base_df$cond == "Body"),]


base_df_armfront
base_df_armbody


x1 = ""
x2 = ""

for(i in 1:length(base_df_armbody$realPattern)){
  if(substr(toString(base_df_armbody$realPattern[i]), 1,1) == "1"){
    x1 = "3"
  }
  else if (substr(toString(base_df_armbody$realPattern[i]), 1,1) == "2"){
    x1 = "1"
  }
  else if (substr(toString(base_df_armbody$realPattern[i]), 1,1) == "3"){
    x1 = "4"
  }
  else if (substr(toString(base_df_armbody$realPattern[i]), 1,1) == "4"){
    x1 = "2"
  }
  
  if(substr(toString(base_df_armbody$realPattern[i]), 2,2) == "1"){
    x2 = "3"
  }
  else if (substr(toString(base_df_armbody$realPattern[i]), 2,2) == "2"){
    x2 = "1"
  }
  else if (substr(toString(base_df_armbody$realPattern[i]), 2,2) == "3"){
    x2 = "4"
  }
  else if (substr(toString(base_df_armbody$realPattern[i]), 2,2) == "4"){
    x2 = "2"
  }
  
  modified = strtoi(paste(x1,x2,sep=""))
  base_df_armbody$realPattern[i] = modified
}






x1 = ""
x2 = ""

for(i in 1:length(base_df_armbody$userAnswer)){
  if(substr(toString(base_df_armbody$userAnswer[i]), 1,1) == "1"){
    x1 = "3"
  }
  else if (substr(toString(base_df_armbody$userAnswer[i]), 1,1) == "2"){
    x1 = "1"
  }
  else if (substr(toString(base_df_armbody$userAnswer[i]), 1,1) == "3"){
    x1 = "4"
  }
  else if (substr(toString(base_df_armbody$userAnswer[i]), 1,1) == "4"){
    x1 = "2"
  }
  
  if(substr(toString(base_df_armbody$userAnswer[i]), 2,2) == "1"){
    x2 = "3"
  }
  else if (substr(toString(base_df_armbody$userAnswer[i]), 2,2) == "2"){
    x2 = "1"
  }
  else if (substr(toString(base_df_armbody$userAnswer[i]), 2,2) == "3"){
    x2 = "4"
  }
  else if (substr(toString(base_df_armbody$userAnswer[i]), 2,2) == "4"){
    x2 = "2"
  }
  
  modified = strtoi(paste(x1,x2,sep=""))
  base_df_armbody$userAnswer[i] = modified
}

base_df_armbody

base_df = c()
base_df = rbind(base_df, base_df_armfront)

base_df


realPattern_first = as.integer(substr(as.character(base_df$realPattern),2,2))
userAnswer_first = as.integer(substr(as.character(base_df$userAnswer),2,2))
#realPattern_first = as.integer(substr(as.character(base_df$realPattern),3,3))
#userAnswer_first = as.integer(substr(as.character(base_df$userAnswer),3,3))

realPattern_one = c()
userAnswer_one = c()

realPattern_one = c(realPattern_one, realPattern_first)
userAnswer_one = c(userAnswer_one, userAnswer_first)

str(realPattern_one)
str(userAnswer_one)

# Confusion matrix
#actual_first <- as.factor(realPattern_one)
#predicted_first <- as.factor(userAnswer_one)
actual_first <- as.factor(base_df$realPattern)
predicted_first <- as.factor(base_df$userAnswer)

str(actual_first)
str(predicted_first)

actual_first
predicted_first


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

# °¡Àå ConfusionÀÌ ¸¹ÀÌ ÀÏ¾î³­ ÆÐÅÏ Ã£±â
all = sum(cm_df$Freq)
correct = sum(cm_df[which(cm_df$Prediction==cm_df$Reference),]$Freq)
wrong = all -correct

# ÆÈ ¾ÕÂÊ
conf1 = cm_df[which(cm_df$Prediction == 14 & cm_df$Reference==12),]$Freq
conf2 = cm_df[which(cm_df$Prediction == 41 & cm_df$Reference==21),]$Freq
conf3 = cm_df[which(cm_df$Prediction == 21 & cm_df$Reference==23),]$Freq
conf4 = cm_df[which(cm_df$Prediction == 43 & cm_df$Reference==23),]$Freq
conf5 = cm_df[which(cm_df$Prediction == 12 & cm_df$Reference==32),]$Freq
conf6 = cm_df[which(cm_df$Prediction == 34 & cm_df$Reference==32),]$Freq
conf7 = cm_df[which(cm_df$Prediction == 14 & cm_df$Reference==34),]$Freq
conf8 = cm_df[which(cm_df$Prediction == 41 & cm_df$Reference==43),]$Freq

# ÆÈ ¾ÕÂÊ (¹Ý´ë)
# conf1 = cm_df[which(cm_df$Prediction == 32 & cm_df$Reference==12),]$Freq
# conf2 = cm_df[which(cm_df$Prediction == 34 & cm_df$Reference==14),]$Freq
# conf3 = cm_df[which(cm_df$Prediction == 12 & cm_df$Reference==14),]$Freq
# conf4 = cm_df[which(cm_df$Prediction == 23 & cm_df$Reference==21),]$Freq
# conf5 = cm_df[which(cm_df$Prediction == 32 & cm_df$Reference==34),]$Freq
# conf6 = cm_df[which(cm_df$Prediction == 43 & cm_df$Reference==41),]$Freq
# conf7 = cm_df[which(cm_df$Prediction == 21 & cm_df$Reference==41),]$Freq
# conf8 = cm_df[which(cm_df$Prediction == 23 & cm_df$Reference==43),]$Freq

# ÆÈ ¸öÂÊ
#conf1 = cm_df[which(cm_df$Prediction == 14 & cm_df$Reference==13),]$Freq
#conf2 = cm_df[which(cm_df$Prediction == 13 & cm_df$Reference==23),]$Freq
#conf3 = cm_df[which(cm_df$Prediction == 24 & cm_df$Reference==23),]$Freq
#conf4 = cm_df[which(cm_df$Prediction == 14 & cm_df$Reference==24),]$Freq
#conf5 = cm_df[which(cm_df$Prediction == 41 & cm_df$Reference==31),]$Freq
#conf6 = cm_df[which(cm_df$Prediction == 31 & cm_df$Reference==32),]$Freq
#conf7 = cm_df[which(cm_df$Prediction == 42 & cm_df$Reference==32),]$Freq
#conf8 = cm_df[which(cm_df$Prediction == 41 & cm_df$Reference==42),]$Freq

# ÆÈ ¸öÂÊ (¹Ý´ë)
# conf1 = cm_df[which(cm_df$Prediction == 23 & cm_df$Reference==13),]$Freq
# conf2 = cm_df[which(cm_df$Prediction == 24 & cm_df$Reference==14),]$Freq
# conf3 = cm_df[which(cm_df$Prediction == 13 & cm_df$Reference==14),]$Freq
# conf4 = cm_df[which(cm_df$Prediction == 23 & cm_df$Reference==24),]$Freq
# conf5 = cm_df[which(cm_df$Prediction == 32 & cm_df$Reference==31),]$Freq
# conf6 = cm_df[which(cm_df$Prediction == 42 & cm_df$Reference==41),]$Freq
# conf7 = cm_df[which(cm_df$Prediction == 31 & cm_df$Reference==41),]$Freq
# conf8 = cm_df[which(cm_df$Prediction == 32 & cm_df$Reference==42),]$Freq

expected_wrong = conf1+conf2+conf3+conf4+conf5+conf6+conf7+conf8

all
correct
wrong
expected_wrong

16 / 182

11 / 394

high_confusion_pattern = c()
nrow(cm_df)
for(i in 1:nrow(cm_df)){
  if(cm_df$Prediction[i] != cm_df$Reference[i])
  {
    high_confusion_pattern = append(high_confusion_pattern, i)
  }
  
}
high_confusion_pattern

