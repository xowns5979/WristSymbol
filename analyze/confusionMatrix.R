library(ggcorrplot)
library(caret)
library(e1071)
library(readxl)
library(ggplot2)
library(dplyr)


# Names
names = c("p1","p2","p3","p4",
          "p5","p6","p7","p8")

# 1. 1 Letter Accuracy [%]  
base_df = data.frame()
for (i in 1:8){
  file_name = paste("data/",names[i],"_exp.csv",sep="")
  file_data = read.csv(file_name, header=T, stringsAsFactors = F)
  file_data$name = names[i]
  base_df = rbind(base_df,file_data)
}
base_df$name = factor(base_df$name, levels=names)

base_df

# Confusion matrix
actual_first <- as.factor(substr(base_df$pattern,3,3))
predicted_first <- as.factor(base_df$answer)

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
