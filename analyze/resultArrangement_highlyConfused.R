library(dplyr)

record_df = data.frame()

# Names
names = c("p1","p2","p3","p4","p5","p6","p7","p8","p9","p10","p11","p12")
cond = c("A(Baseline1)","B(Approach)","C(Baseline2)")
#cond = c("Baseline1","Approach","Baseline2")
mode = c("training","main")

highConfusionPattern = c(13,23,24,31,32,41)

# 1. 1 Letter Accuracy [%]  
for (i in 1:12){
  count = -1
  baseline1 = -1
  approach = -1
  baseline2 = -1
  for (j in 1:3){
    
    base_df = data.frame()
    
    file_name = paste("data/",names[i] ,"_",cond[j],"_",mode[2],".csv",sep="")
    file_data = read.csv(file_name, header=T, stringsAsFactors = F)
    base_df = rbind(base_df,file_data)
    
    base_df$id = factor(base_df$id, levels=names)
    base_df
    result = group_by(base_df, id, realPattern) %>%
      summarise(
        count = n(),
        correct = mean(correct)*100
      )
    #print(result,n=36)
    
    if(j==1)
    {
      #highConfusionPattern = result[result$correct < mean(result$correct), ]$realPattern
      #print(highConfusionPattern)
      count = length(highConfusionPattern)
      baseline1 = mean(result[result$realPattern %in% highConfusionPattern, ]$correct)
    }
    else if(j==2)
    {
      approach = mean(result[result$realPattern %in% highConfusionPattern, ]$correct)
    }
    else if(j==3)
    {
      baseline2 = mean(result[result$realPattern %in% highConfusionPattern, ]$correct)
      add_df = data.frame("Count"=count, "Baseline1"=baseline1, "Approach"=approach, "Baseline2"=baseline2)
      #print(add_df)
      record_df = rbind(record_df,add_df)
      
    }
  }
}

print(record_df)
write.csv(record_df, "result.csv")


