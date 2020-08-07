library(ggcorrplot)
library(caret)
library(e1071)
library(readxl)
library(ggplot2)
library(dplyr)


# Names
names = c("p2","p3","p4","p6","p7","p8","p9","p10","p11","p12")
cond = c("Distal","Body")
#cond = c("Distal","Body")
mode = c("training","main")

# 1. 1 Letter Accuracy [%]  
base_df = data.frame()
for (i in 1:10){
  file_name = paste("Exp7_data/",names[i],"_",cond[2],"_",mode[2],".csv",sep="")
  file_data = read.csv(file_name, header=T, stringsAsFactors = F)
  base_df = rbind(base_df,file_data)
}
base_df$id = factor(base_df$id, levels=names)
base_df

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

# ÆÈ ¸öÂÊ(ÁÖÈ²»ö)
conf1 = cm_df[which(cm_df$Prediction == 31 & cm_df$Reference==12),]$Freq
conf2 = cm_df[which(cm_df$Prediction == 32 & cm_df$Reference==12),]$Freq
conf3 = cm_df[which(cm_df$Prediction == 34 & cm_df$Reference==13),]$Freq
conf4 = cm_df[which(cm_df$Prediction == 12 & cm_df$Reference==14),]$Freq
conf5 = cm_df[which(cm_df$Prediction == 32 & cm_df$Reference==14),]$Freq
conf6 = cm_df[which(cm_df$Prediction == 34 & cm_df$Reference==14),]$Freq
conf7 = cm_df[which(cm_df$Prediction == 13 & cm_df$Reference==21),]$Freq
conf8 = cm_df[which(cm_df$Prediction == 23 & cm_df$Reference==21),]$Freq
conf9 = cm_df[which(cm_df$Prediction == 14 & cm_df$Reference==23),]$Freq
conf10 = cm_df[which(cm_df$Prediction == 12 & cm_df$Reference==24),]$Freq
conf11 = cm_df[which(cm_df$Prediction == 43 & cm_df$Reference==31),]$Freq
conf12 = cm_df[which(cm_df$Prediction == 41 & cm_df$Reference==32),]$Freq
conf13 = cm_df[which(cm_df$Prediction == 32 & cm_df$Reference==34),]$Freq
conf14 = cm_df[which(cm_df$Prediction == 42 & cm_df$Reference==34),]$Freq
conf15 = cm_df[which(cm_df$Prediction == 21 & cm_df$Reference==41),]$Freq
conf16 = cm_df[which(cm_df$Prediction == 23 & cm_df$Reference==41),]$Freq
conf17 = cm_df[which(cm_df$Prediction == 43 & cm_df$Reference==41),]$Freq
conf18 = cm_df[which(cm_df$Prediction == 21 & cm_df$Reference==42),]$Freq
conf19 = cm_df[which(cm_df$Prediction == 23 & cm_df$Reference==43),]$Freq
conf20 = cm_df[which(cm_df$Prediction == 24 & cm_df$Reference==43),]$Freq


expected_wrong = conf1+conf2+conf3+conf4+conf5+conf6+conf7+conf8+conf9+conf10+conf11+conf12+conf13+conf14+conf15+conf16+conf17+conf18+conf19+conf20
#expected_wrong = conf1+conf2+conf3+conf4+conf5+conf6+conf7+conf8
print(expected_wrong)


# ÆÈ ¾ÕÂÊ
# conf1 = cm_df[which(cm_df$Prediction == 14 & cm_df$Reference==12),]$Freq
# conf2 = cm_df[which(cm_df$Prediction == 41 & cm_df$Reference==21),]$Freq
# conf3 = cm_df[which(cm_df$Prediction == 21 & cm_df$Reference==23),]$Freq
# conf4 = cm_df[which(cm_df$Prediction == 43 & cm_df$Reference==23),]$Freq
# conf5 = cm_df[which(cm_df$Prediction == 12 & cm_df$Reference==32),]$Freq
# conf6 = cm_df[which(cm_df$Prediction == 34 & cm_df$Reference==32),]$Freq
# conf7 = cm_df[which(cm_df$Prediction == 14 & cm_df$Reference==34),]$Freq
# conf8 = cm_df[which(cm_df$Prediction == 41 & cm_df$Reference==43),]$Freq

# ÆÈ ¾ÕÂÊ (ÁÖÈ²»ö)
# conf1 = cm_df[which(cm_df$Prediction == 24 & cm_df$Reference==12),]$Freq
# conf2 = cm_df[which(cm_df$Prediction == 21 & cm_df$Reference==13),]$Freq
# conf3 = cm_df[which(cm_df$Prediction == 23 & cm_df$Reference==13),]$Freq
# conf4 = cm_df[which(cm_df$Prediction == 13 & cm_df$Reference==14),]$Freq
# conf5 = cm_df[which(cm_df$Prediction == 23 & cm_df$Reference==14),]$Freq
# conf6 = cm_df[which(cm_df$Prediction == 24 & cm_df$Reference==14),]$Freq
# conf7 = cm_df[which(cm_df$Prediction == 42 & cm_df$Reference==21),]$Freq
# conf8 = cm_df[which(cm_df$Prediction == 41 & cm_df$Reference==23),]$Freq
# conf9 = cm_df[which(cm_df$Prediction == 23 & cm_df$Reference==24),]$Freq
# conf10 = cm_df[which(cm_df$Prediction == 43 & cm_df$Reference==24),]$Freq
# conf11 = cm_df[which(cm_df$Prediction == 12 & cm_df$Reference==31),]$Freq
# conf12 = cm_df[which(cm_df$Prediction == 32 & cm_df$Reference==31),]$Freq
# conf13 = cm_df[which(cm_df$Prediction == 14 & cm_df$Reference==32),]$Freq
# conf14 = cm_df[which(cm_df$Prediction == 13 & cm_df$Reference==34),]$Freq
# conf15 = cm_df[which(cm_df$Prediction == 31 & cm_df$Reference==41),]$Freq
# conf16 = cm_df[which(cm_df$Prediction == 32 & cm_df$Reference==41),]$Freq
# conf17 = cm_df[which(cm_df$Prediction == 42 & cm_df$Reference==41),]$Freq
# conf18 = cm_df[which(cm_df$Prediction == 32 & cm_df$Reference==42),]$Freq
# conf19 = cm_df[which(cm_df$Prediction == 34 & cm_df$Reference==42),]$Freq
# conf20 = cm_df[which(cm_df$Prediction == 31 & cm_df$Reference==43),]$Freq


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




high_confusion_pattern = c()
nrow(cm_df)
for(i in 1:nrow(cm_df)){
  if(cm_df$Prediction[i] != cm_df$Reference[i])
  {
    high_confusion_pattern = append(high_confusion_pattern, i)
  }
  
}
high_confusion_pattern

