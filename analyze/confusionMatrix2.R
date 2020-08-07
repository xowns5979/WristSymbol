library(ggcorrplot)
library(caret)
library(e1071)
library(readxl)
library(ggplot2)
library(dplyr)


# Names
names = c("p1","p2","p3","p4","p5","p6","p7","p8","p9","p10","p11","p12")
cond = c("A(Baseline1)","B(Approach)","C(Baseline2)")
#cond = c("Distal","Body")
mode = c("training","main")
# 1. 1 Letter Accuracy [%]
for (i in 1:12){
  base_df = data.frame()
  
  file_name = paste("data/",names[i],"_",cond[2],"_",mode[2],".csv",sep="")
  file_data = read.csv(file_name, header=T, stringsAsFactors = F)
  base_df = rbind(base_df,file_data)
  
  # Confusion matrix
  actual_first <- as.factor(base_df$realPattern)
  predicted_first <- as.factor(base_df$userAnswer)
  
  #str(actual_first)
  #str(predicted_first)
  
  #actual_first
  #predicted_first
  
  
  cm_first <- confusionMatrix(predicted_first, actual_first)
  cm_temp = cm_first$table 
  
  cm_df <- as.data.frame(as.matrix(cm_temp))
  print(cm_df)
  
  # °¡Àå ConfusionÀÌ ¸¹ÀÌ ÀÏ¾î³­ ÆÐÅÏ Ã£±â
  #all = sum(cm_df$Freq)
  #correct = sum(cm_df[which(cm_df$Prediction==cm_df$Reference),]$Freq)
  #wrong = all -correct
  
  # ÆÈ ¸öÂÊ
  conf1 = cm_df[which(cm_df$Prediction == 14 & cm_df$Reference==13),]$Freq
  conf2 = cm_df[which(cm_df$Prediction == 13 & cm_df$Reference==23),]$Freq
  conf3 = cm_df[which(cm_df$Prediction == 24 & cm_df$Reference==23),]$Freq
  conf4 = cm_df[which(cm_df$Prediction == 14 & cm_df$Reference==24),]$Freq
  conf5 = cm_df[which(cm_df$Prediction == 41 & cm_df$Reference==31),]$Freq
  conf6 = cm_df[which(cm_df$Prediction == 31 & cm_df$Reference==32),]$Freq
  conf7 = cm_df[which(cm_df$Prediction == 42 & cm_df$Reference==32),]$Freq
  conf8 = cm_df[which(cm_df$Prediction == 41 & cm_df$Reference==42),]$Freq
  
  expected_wrong = conf1+conf2+conf3+conf4+conf5+conf6+conf7+conf8
  
  print(names[i])
  print(expected_wrong)
}


# ggplot(cm_df) +
#   geom_tile(aes(x=Prediction, y=Reference, fill=Freq)) +
#   coord_equal() +
#   geom_text(aes(x=Prediction, y=Reference, label = sprintf("%1.0f", Freq)), vjust = 0.3, size= 4)+
#   labs(title="", x="User Typed Pattern", y="Actual Pattern") +
#   scale_y_discrete(limits = rev(levels(cm_df$Reference))) +
#   scale_fill_gradient(low="white", high="slategrey") +
#   theme_bw()



# ÆÈ ¾ÕÂÊ
# conf1 = cm_df[which(cm_df$Prediction == 14 & cm_df$Reference==12),]$Freq
# conf2 = cm_df[which(cm_df$Prediction == 41 & cm_df$Reference==21),]$Freq
# conf3 = cm_df[which(cm_df$Prediction == 21 & cm_df$Reference==23),]$Freq
# conf4 = cm_df[which(cm_df$Prediction == 43 & cm_df$Reference==23),]$Freq
# conf5 = cm_df[which(cm_df$Prediction == 12 & cm_df$Reference==32),]$Freq
# conf6 = cm_df[which(cm_df$Prediction == 34 & cm_df$Reference==32),]$Freq
# conf7 = cm_df[which(cm_df$Prediction == 14 & cm_df$Reference==34),]$Freq
# conf8 = cm_df[which(cm_df$Prediction == 41 & cm_df$Reference==43),]$Freq


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

