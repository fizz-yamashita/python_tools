# coding: utf-8
#設計
#

lineNumber = 1
outputString = ""

# コメント行がチェックする
def checkComment(source):
	# タブを削除して、最初の文字が"/"だった場合は、コメント行なので無視をする
	tabReplaceSource = source.replace("	", "")
	pos = tabReplaceSource.find("/", 0)
	result = True
	if pos == 0:
		result = True
	else:
		result = False
	return result

# if else for(each) while の後に、スペースがなかったら、警告
def checkLogicalOperatorAfterSpace(source):
	global outputString
	
	length = len(source)
	pos = source.find("if", 0)
	if pos != -1:
		if (pos+1+2) < length:
			nextChar = source[pos+2]
			if nextChar.find(" ", 0) != 0:
				outputString += str(lineNumber) + " : " + "checkLogicalOperatorAfterSpace:next" + "\n"

	pos = source.find("else", 0) 
	if pos != -1:
		if (pos+1+4) < length:
			nextChar = source[pos+4]
			if nextChar.find(" ", 0) != 0:
				outputString += str(lineNumber) + " : " + "checkLogicalOperatorAfterSpace:next" + "\n"
	
	pos = source.find("else if", 0) 
	if pos != -1:
		if (pos+1+7) < length:
			nextChar = source[pos+7]
			if nextChar.find(" ", 0) != 0:
				outputString += str(lineNumber) + " : " + "checkLogicalOperatorAfterSpace:next" + "\n"

	pos = source.find("for", 0) 
	if pos != -1:
		if (pos+1+3) < length:
			nextChar = source[pos+3]
			if nextChar.find(" ", 0) != 0:
				outputString += str(lineNumber) + " : " + "checkLogicalOperatorAfterSpace:next" + "\n"
	
	pos = source.find("foreach", 0) 
	if pos != -1:
		if (pos+1+7) < length:
			nextChar = source[pos+7]
			if nextChar.find(" ", 0) != 0:
				outputString += str(lineNumber) + " : " + "checkLogicalOperatorAfterSpace:next" + "\n"
	
	pos = source.find("while", 0) 
	if pos != -1:
		if (pos+1+5) < length:
			nextChar = source[pos+5]
			if nextChar.find(" ", 0) != 0:
				outputString += str(lineNumber) + " : " + "checkLogicalOperatorAfterSpace:next" + "\n"

# if else for(each) while があるかどうか
def checkLogicalOperator(source):
	result = False
	pos = source.find("if", 0) 
	if pos != -1:
		result = True

	pos = source.find("else", 0) 
	if pos != -1:
		result = True
	
	pos = source.find("else if", 0) 
	if pos != -1:
		result = True

	pos = source.find("for", 0) 
	if pos != -1:
		result = True
	
	pos = source.find("foreach", 0) 
	if pos != -1:
		result = True
	
	pos = source.find("while", 0) 
	if pos != -1:
		result = True
	
	return result

# ４文字インデント、インデントにはスペースではなくタブを使用する。
def check4indent(source):
	global outputString
	if source.find("    ", 0) != -1:
		outputString += str(lineNumber) + " : " + "check4indent" + "\n"

# "{"を見つけた場合は、前後に半角スペースがなかったら、何かおかしい
def checkMiddleBracket(source):
	if checkComment(source) == True:
		return

	global outputString
	pos = source.find("{", 0) 
	length = len(source)
	if pos != -1:
		if (pos-1) >= 0:
			prevChar = source[pos-1]
			if prevChar.find(" ", 0) == -1:
				outputString += str(lineNumber) + " : " + "checkMiddleBracket:prev" + "\n"
		
		if (pos+1+1) < length:
			nextChar = source[pos+1]
			if nextChar.find("\n", 0) != 0 or nextChar.find("\r\n", 0) != 0 or nextChar.find("\r", 0) != 0:
				if prevChar.find(" ", 0) == -1:
					outputString += str(lineNumber) + " : " + "checkMiddleBracket:next" + "\n"

# 中括弧を除く開き括弧('('、'<'、'[')の後ろにはスペースを入れない。閉じ括弧の前にもスペースを入れない。
def checkOtherBracket(source):
	if checkComment(source) == True:
		return
	
	global outputString
	pos = source.find("<", 0) 
	if pos != -1:
		prevChar = source[pos-1]
		if prevChar.find(" ", 0) != -1:
			outputString += str(lineNumber) + " : " + "checkOtherBracket : < " + "\n"

	pos = source.find("(", 0) 
	if pos != -1:
		prevChar = source[pos-1]
		if prevChar.find(" ", 0) != -1:
			# if等がなければ、表示
			if checkLogicalOperator(source) == False:
				outputString += str(lineNumber) + " : " + "checkOtherBracket : ( " + "\n"

	pos = source.find("[", 0) 
	if pos != -1:
		prevChar = source[pos-1]
		if prevChar.find(" ", 0) != -1:
			outputString += str(lineNumber) + " : " + "checkOtherBracket : [ " + "\n"


# 保存
def saveOutputString():
	global outputString
	f = open("input/output.cs", 'wb') # 書き込みモードで開く
	f.write(outputString) # 引数の文字列をファイルに書き込む
	f.close() # ファイルを閉じる

f = open("input/input.cs")
lines2 = f.readlines() # 1行毎にファイル終端まで全て読む(改行文字も含まれる)
f.close()
for line in lines2:
	check4indent(line)
	checkMiddleBracket(line)
	checkOtherBracket(line)
	checkLogicalOperatorAfterSpace(line)
	lineNumber = lineNumber + 1

print outputString
saveOutputString()

#tes = "  {"
#tespos = tes.find("{", 0)
#print tespos # 2
#print len(tes) # 3



#str = " "
##otherstr = str[1]
#index = str.find("a", 0);
#print index

#fileName = ""
#className = ""
#parameterList = []
#sourceString = ""
#
## ファイル読み込み検証
##f = open("input/input.txt")
##data1 = f.read()
##f.close()
##
##print(data1)
#
## _の次の文字を大文字にして、_を削除する
#def fileNameCustom(intext, offset):
#    pos = intext.find("_", offset) 
#    if pos == -1:
#        ret = intext.replace("_", "")
#        return ret
#    else:
#        li = list(intext)
#        upper = intext[pos+1].upper()
#        li[pos+1] = upper
#        intext = "".join(li)
#        return fileNameCustom(intext, pos+1)
#
## 文字列を検証して、パラメータリストの作成
#def createParameter(intext, offset, culmns):
#    global parameterList
#    start = intext.find("|", offset)
#    if start != -1:
#        end = intext.find("|", start+1)
#        if end != -1:
#            string = intext[start+1:end]
#            parameterList.append(string)
#            culmns = culmns + 1
#            if culmns >= 3:
#                return
#            createParameter(intext, end, culmns)
#
## ４文字インデント、インデントにはスペースではなくタブを使用する。
#def check4indent(source):
#	if source.find("    ", 0) != -1
#
##teststring = "| aaaa| bbb| cc| D"
##createParameter(teststring, 0)
##print parameterList
#
#f = open("input/input.txt")
#lines2 = f.readlines() # 1行毎にファイル終端まで全て読む(改行文字も含まれる)
#f.close()
#for line in lines2:
#    print line
#    if line.find("h3", 0) != -1:
#        #タイトルから、クラス名とファイル名作成作成
#        output1 = line.replace("h3. ", "");
#        output2 = fileNameCustom(output1, 0)
#        output2 = output2.replace('\xEF\xBB\xBF', "")#今は邪魔なので BOM削除
#        #頭文字大文字に
#        li = list(output2)
#        upper = li[0].upper()
#        li[0] = upper
#        output2 = "".join(li)
#        fileName = output2
#        className = output2
#    
#    elif line.find("|", 0) != -1 and line.find("_.field name", 0) == -1:
#        #リスト情報から、パラメータ作成
#        createParameter(line, 0, 0)
#
##-----ここに、クラス名作成
#className = className.strip()
##-----
#
## ここから、Listを回して、変数作成
#dataKind = 3
#dataNum = len(parameterList) / dataKind
#print len(parameterList)
#
#for loop in range(dataNum) :
#    offset = loop * dataKind
#    sourceString += "	/// <summary>" + parameterList[offset+2] + "</summary>" + "\n"
#    if parameterList[offset+1].find("int") != -1:
#        sourceString += "	internal readonly " + parameterList[offset+1] + " " + parameterList[offset+0] + " = 0;" + "\n"
#    elif parameterList[offset+1].find("string") != -1:
#        sourceString += "	internal readonly " + parameterList[offset+1] + " " + parameterList[offset+0] + ' = "";' + "\n"
#
#
## ここまで
#
#sourceString += "" + "\n"
#sourceString += "}" + "\n"
#
#fileName = fileName.strip()
#
#f = open(fileName + ".cs", 'wb') # 書き込みモードで開く
#f.write(sourceString) # 引数の文字列をファイルに書き込む
#f.close() # ファイルを閉じる
#
