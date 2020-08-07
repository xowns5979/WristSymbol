library(dplyr)


# Names
names = c("p1","p2","p3","p4","p5","p6","p7","p8","p9","p10","p11","p12")
cond = c("Baseline1","Approach","Baseline2")
mode = c("training","main")
# 1. 1 Letter Accuracy [%]  
used_name = names[12]


base_df = data.frame()
for (i in 1:1){
  file_name = paste("Exp6_data/",used_name ,"_",cond[3],"_",mode[2],".csv",sep="")
  file_data = read.csv(file_name, header=T, stringsAsFactors = F)
  base_df = rbind(base_df,file_data)
}
base_df$id = factor(base_df$id, levels=names)
base_df$reaction_time = base_df$enterstamp - base_df$playendstamp
base_df$diffCount_1 = 0
base_df$diffCount_2 = 0
base_df$diffCount_3 = 0

for(i in 1:length(base_df$userAnswer)){
  diffCount = 0
  if(substr(toString(base_df$realPattern[i]), 1,1) != substr(toString(base_df$userAnswer[i]), 1,1)){
    diffCount = diffCount + 1
  }
  if(substr(toString(base_df$realPattern[i]), 2,2) != substr(toString(base_df$userAnswer[i]), 2,2)){
    diffCount = diffCount + 1
  }
  if(substr(toString(base_df$realPattern[i]), 3,3) != substr(toString(base_df$userAnswer[i]), 3,3)){
    diffCount = diffCount + 1
  }

  if(diffCount == 1)
  {
    base_df$diffCount_1[i] = 1
  }
  else if(diffCount == 2)
  {
    base_df$diffCount_2[i] = 1
  }
  else if(diffCount == 3)
  {
    base_df$diffCount_3[i] = 1
  }
}
base_df
write.csv(base_df,paste(used_name ,"_",cond[3],"_",mode[2],".csv",sep=""),row.names=FALSE )





result = group_by(base_df, id) %>%
  summarise(
    count = n(),
    correct = sum(correct)*100
    #rt = mean(reaction_time)
    #sd = sd(correct, na.rm = TRUE)*100
  )
print(result,n=36)
sd(result$correct)

