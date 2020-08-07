library(ggcorrplot)
library(caret)
library(e1071)
library(readxl)
library(ggplot2)
library(dplyr)


# Names
names = c("p1","p2","p3","p4","p5","p6","p7","p8","p9","p10","p11","p12")
cond = c("armBody","armFront")
mode = c("training","main")


# 1. 1 Letter Accuracy [%]  
base_df = data.frame()
for (i in 1:10){
  file_name = paste("Exp14_data/",names[i],"_",cond[2],"_",mode[2],".csv",sep="")
  file_data = read.csv(file_name, header=T, stringsAsFactors = F)
  base_df = rbind(base_df,file_data)
}
base_df$id = factor(base_df$id, levels=names)
base_df

# Confusion matrix
actual_first <- as.factor(base_df$realPattern)
predicted_first <- as.factor(base_df$userAnswer)

actual_first_1 = strtoi(substr(actual_first,1,1))
actual_first_2 = strtoi(substr(actual_first,2,2))
actual_first_3 = strtoi(substr(actual_first,3,3))

predicted_first_1 = strtoi(substr(predicted_first,1,1))
predicted_first_2 = strtoi(substr(predicted_first,2,2))
predicted_first_3 = strtoi(substr(predicted_first,3,3))

actual_first_real = as.factor(c(actual_first_1, actual_first_2, actual_first_3))
predicted_first_real = as.factor(c(predicted_first_1, predicted_first_2, predicted_first_3))



str(actual_first_real)
str(predicted_first_real)

length(actual_first_real)
length(predicted_first_real)


cm_first <- confusionMatrix(predicted_first_real, actual_first_real)
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

