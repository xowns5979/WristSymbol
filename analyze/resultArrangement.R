library(dplyr)


# Names
names = c("TJ","YB")
orientation = c("handNorth","watchNorth")
armpose = c("armFront","armBody","armDown")
mode = c("training","main")
# 1. 1 Letter Accuracy [%]  

base_df = data.frame()
for (i in 2:2){
  for(j in 1:2)
  {
    for(k in 1:3)
    {
      file_name = paste("Exp13_data/",names[i] ,"_",orientation[j],"_",armpose[k],"_",mode[2],".csv",sep="")
      file_data = read.csv(file_name, header=T, stringsAsFactors = F)
      base_df = rbind(base_df,file_data)
    }
  }
}
base_df$id = factor(base_df$id, levels=names)
base_df

result = group_by(base_df, id, orientation, armpose) %>%
  summarise(
    count = n(),
    correct = mean(correct)*100
    #diff_1 = sum(diffCount_1),
    #diff_2 = sum(diffCount_2),
    #diff_3 = sum(diffCount_3)
    #rt = mean(reaction_time)
    #sd = sd(correct, na.rm = TRUE)*100
  )
print(result,n=36)
