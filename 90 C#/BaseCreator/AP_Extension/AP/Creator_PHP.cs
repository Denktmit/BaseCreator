using System;
using System.Security;
using System.Collections.Generic;
using BaseCreator_Core.Helper;

namespace AP_Extension.AP {

  public class Creator_PHP {

    #region fields
    private AutoCreator _ac;
    private List<string> _tempFileContent;
    #endregion fields

    #region properties
    public AutoCreator AC { get { return _ac; } set { _ac = value; } }
    public List<string> TempFileContent { get { return _tempFileContent; } set { _tempFileContent = value; } }
    #endregion properties

    #region konstruktor
    public Creator_PHP(AutoCreator inAC) {
      AC = inAC;
      TempFileContent = new List<string>();
    }
    #endregion konstruktor

    #region class_methods
    public void run() {
      CreatePHP();
    }
    #endregion class_methods

    #region create_php
    private void CreatePHP() {
      CreatePHP_callSP();
      CreatePHP_loadTable();
    }
    private void CreatePHP_callSP() {
      Console.WriteLine("  -  CreatePHP_callSP()");
      string filePath = AC.APPath + "\\60_php";
      string fileName = "callSP";
      string fileExt = "php";
      TempFileContent = new List<string>();

      Atf("<?php");
      Atf("");
      Atf("");
      Atf("//Step0 - local variables");
      Atf("header('Access-Control-Allow-Origin: *');");
      Atf("header('Content-Type: application/json; charset=UTF-8');");
      Atf("$stmtPrepared  = 0;");
      Atf("$resultId      = 0;");
      Atf("");
      Atf("");
      Atf("//Step1 - db-connection");
      Atf("$servername  = 'localhost';");
      Atf("$username    = 'administrator';");
      Atf("$password    = 'uhuAdmin1836';");
      Atf("$dbname      = 'dbAktivenplaner';");
      Atf("");
      Atf("");
      Atf("//Step2 - get input parameters");
      Atf("$jsonData    = json_decode(file_get_contents(\"php://input\"), true);");
      Atf("//");
      Atf("$userId      = $jsonData[\"userId\"];");
      Atf("$datenstatus = $jsonData[\"datenstatus\"];");
      Atf("$deleteMode  = $jsonData[\"deleteMode\"];");
      Atf("$table       = $jsonData[\"table\"];");
      Atf("$mode        = $jsonData[\"mode\"];");
      Atf("$xId         = $jsonData[\"xId\"];");
      Atf("$param00     = $jsonData[\"param00\"];");
      Atf("$param01     = $jsonData[\"param01\"];");
      Atf("$param02     = $jsonData[\"param02\"];");
      Atf("$param03     = $jsonData[\"param03\"];");
      Atf("$param04     = $jsonData[\"param04\"];");
      Atf("$param05     = $jsonData[\"param05\"];");
      Atf("$param06     = $jsonData[\"param06\"];");
      Atf("$param07     = $jsonData[\"param07\"];");
      Atf("$param08     = $jsonData[\"param08\"];");
      Atf("$param09     = $jsonData[\"param09\"];");
      Atf("$param10     = $jsonData[\"param10\"];");
      Atf("$param11     = $jsonData[\"param11\"];");
      Atf("$param12     = $jsonData[\"param12\"];");
      Atf("$param13     = $jsonData[\"param13\"];");
      Atf("$param14     = $jsonData[\"param14\"];");
      Atf("$param15     = $jsonData[\"param15\"];");
      Atf("$param16     = $jsonData[\"param16\"];");
      Atf("$param17     = $jsonData[\"param17\"];");
      Atf("$param18     = $jsonData[\"param18\"];");
      Atf("$param19     = $jsonData[\"param19\"];");
      Atf("$param20     = $jsonData[\"param20\"];");
      Atf("$param21     = $jsonData[\"param21\"];");
      Atf("$param22     = $jsonData[\"param22\"];");
      Atf("$param23     = $jsonData[\"param23\"];");
      Atf("$param24     = $jsonData[\"param24\"];");
      Atf("$param25     = $jsonData[\"param25\"];");
      Atf("$param26     = $jsonData[\"param26\"];");
      Atf("$param27     = $jsonData[\"param27\"];");
      Atf("$param28     = $jsonData[\"param28\"];");
      Atf("$param29     = $jsonData[\"param29\"];");
      Atf("$resId       = $jsonData[\"outId\"];");
      Atf("$resultset   = $jsonData[\"resultset\"];");
      Atf("$debug       = $jsonData[\"debug\"];");
      Atf("$debugDepth  = $jsonData[\"debugDepth\"];");
      Atf("$nError      = $jsonData[\"nError\"];");
      Atf("$szError     = $jsonData[\"szError\"];");
      Atf("");
      Atf("");
      Atf("try {");
      Atf("  $conn = new PDO(\"mysql: host=$servername; dbname=$dbname\", $username, $password);");
      Atf("  // set the PDO error mode to exception");
      Atf("  $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);");
      Atf("");
      Atf("  // prepare sql and bind parameters");
      Atf("");
      // mode 8 -> insert
      Atf("  // mode 8 -> insert");
      Atf("  if($mode==8) {");
      Atf("    switch($table) {");
      foreach (var table in AC.DbTables) {
        Atf("      case \"tblAP" + table.TableName + "\":");
        string s = "        $stmt = $conn->prepare(\"CALL spAPInsert" + table.TableName + "(:pUserId, :pDatStat";
        for (int c = 3; c < table.Columns.Count; c++) {
          if (c < 13) {
            s += ", :p0" + (c - 3) + "";
          }
          else {
            s += ", :p" + (c - 3) + "";
          }
        }
        s += ", @resId, :resSet, :debug, :debugDepth, @nError, @szError)\");";
        Atf(s);
        Atf("        $stmt or die('Error preparing statement.');");
        Atf("        $stmt->bindParam(':pUserId',    $userId,       PDO::PARAM_INT);");
        Atf("        $stmt->bindParam(':pDatStat',   $datenstatus,  PDO::PARAM_INT);");
        for (int c = 3; c < table.Columns.Count; c++) {
          s = "        $stmt->bindParam(':p";
          string tmp = "";
          if (c < 13) {
            tmp = "0" + (c - 3);
          }
          else {
            tmp = "" + (c - 3);
          }
          s += tmp + "',        $param" + tmp + ",      " + GetPDOType(table.Columns[c]) + ");   // " + table.Columns[c].Attribut;
          Atf(s);
        }
        Atf("//      $stmt->bindParam(':resultId',   $resultId,     PDO::PARAM_INT);");
        Atf("        $stmt->bindParam(':resSet',     $resultset,    PDO::PARAM_INT);");
        Atf("        $stmt->bindParam(':debug',      $debug,        PDO::PARAM_INT);");
        Atf("        $stmt->bindParam(':debugDepth', $debugDepth,   PDO::PARAM_INT);");
        Atf("//      $stmt->bindParam(':nError',     $nError,       PDO::PARAM_INT);");
        Atf("//      $stmt->bindParam(':szError',    $szError,      PDO::PARAM_STR, 1000);");
        Atf("        $stmtPrepared = 1;");
        Atf("        break;");
      }
      Atf("      default:");
      Atf("        $stmtPrepared = 0;");
      Atf("        break;");
      Atf("    }");
      Atf("  }");
      // mode 9 -> Update
      Atf("  // mode 9 -> Update");
      Atf("  elseif ($mode == 9){");
      Atf("    switch ($table){");
      foreach (var table in AC.DbTables) {
        Atf("      case \"tblAP" + table.TableName + "\":");
        string s = "        $stmt = $conn->prepare(\"CALL spAPUpdate" + table.TableName + "(:pUserId, :pInId, :pDatStat";
        for (int c = 3; c < table.Columns.Count; c++) {
          if (c < 13) {
            s += ", :p0" + (c - 3) + "";
          }
          else {
            s += ", :p" + (c - 3) + "";
          }
        }
        s += ", @resId, :resSet, :debug, :debugDepth, @nError, @szError)\");";
        Atf(s);
        Atf("        $stmt or die('Error preparing statement.');");
        Atf("        $stmt->bindParam(':pUserId',    $userId,       PDO::PARAM_INT);");
        Atf("        $stmt->bindParam(':pInId',      $xId,          PDO::PARAM_INT);");
        Atf("        $stmt->bindParam(':pDatStat',   $datenstatus,  PDO::PARAM_INT);");
        for (int c = 3; c < table.Columns.Count; c++) {
          s = "        $stmt->bindParam(':p";
          string tmp = "";
          if (c < 13) {
            tmp = "0" + (c - 3);
          }
          else {
            tmp = "" + (c - 3);
          }
          s += tmp + "',        $param" + tmp + ",      " + GetPDOType(table.Columns[c]) + ");   // " + table.Columns[c].Attribut;
          Atf(s);
        }
        Atf("//      $stmt->bindParam(':resultId',   $resultId,     PDO::PARAM_INT);");
        Atf("        $stmt->bindParam(':resSet',     $resultset,    PDO::PARAM_INT);");
        Atf("        $stmt->bindParam(':debug',      $debug,        PDO::PARAM_INT);");
        Atf("        $stmt->bindParam(':debugDepth', $debugDepth,   PDO::PARAM_INT);");
        Atf("//      $stmt->bindParam(':nError',     $nError,       PDO::PARAM_INT);");
        Atf("//      $stmt->bindParam(':szError',    $szError,      PDO::PARAM_STR, 1000);");
        Atf("        $stmtPrepared = 1;");
        Atf("        break;");
      }
      Atf("      default:");
      Atf("        $stmtPrepared = 0;");
      Atf("        break;");
      Atf("    }");
      Atf("  }");
      // mode 10 -> Delete
      Atf("  // mode 10 -> Delete");
      Atf("  elseif ($mode == 10){");
      Atf("    switch ($table){");
      foreach (var table in AC.DbTables) {
        Atf("      case \"tblAP" + table.TableName + "\":");
        string s = "        $stmt = $conn->prepare(\"CALL spAPDelete" + table.TableName + "(:pUserId, :pInId";
        s += ", @resId, :resSet, :debug, :debugDepth, @nError, @szError, :deleteMode)\");";
        Atf(s);
        Atf("        $stmt or die('Error preparing statement.');");
        Atf("        $stmt->bindParam(':pUserId',    $userId,       PDO::PARAM_INT);");
        Atf("        $stmt->bindParam(':pInId',      $xId,          PDO::PARAM_INT);");
        Atf("//      $stmt->bindParam(':resultId',   $resultId,     PDO::PARAM_INT);");
        Atf("        $stmt->bindParam(':resSet',     $resultset,    PDO::PARAM_INT);");
        Atf("        $stmt->bindParam(':debug',      $debug,        PDO::PARAM_INT);");
        Atf("        $stmt->bindParam(':debugDepth', $debugDepth,   PDO::PARAM_INT);");
        Atf("//      $stmt->bindParam(':nError',     $nError,       PDO::PARAM_INT);");
        Atf("//      $stmt->bindParam(':szError',    $szError,      PDO::PARAM_STR, 1000);");
        Atf("        $stmt->bindParam(':deleteMode', $deleteMode,   PDO::PARAM_INT);");
        Atf("        $stmtPrepared = 1;");
        Atf("        break;");
      }
      Atf("      default:");
      Atf("        $stmtPrepared = 0;");
      Atf("        break;");
      Atf("    }");
      Atf("  }");
      // mode invalid
      Atf("  // mode invalid");
      Atf("  else{");
      Atf("    $stmtPrepared = 0;");
      Atf("  }");
      Atf("");
      Atf("");
      Atf("//  Step6 - Execute DB_Connection");
      Atf("  if($stmtPrepared == 1){");
      Atf("    $result = $stmt->execute();");
      Atf("    $result or die('Error executing statement.');");
      Atf("  }");
      Atf("  else{");
      Atf("    echo \"Error - Statement was not prepared.\";");
      Atf("  }");
      Atf("");
      Atf("");
      Atf("//  Step7 - Get Results");
      Atf("  $sql = \"SELECT @resId   AS phpResId");
      Atf("               , @nError  AS phpNError");
      Atf("               , @szError AS phpSzError");
      Atf("          FROM dual\";");
      Atf("  $results = current($conn->query($sql)->fetchAll());");
      Atf("  $resId   = $results['phpResId'];");
      Atf("  $nError  = $results['phpNError'];");
      Atf("  $szError = $results['phpSzError'];");
      Atf("");
      Atf("");
      Atf("//  Step8 - Return result");
      Atf("  echo \"{ \" . json_encode(resultId) . \":\" . json_encode($resId);");
      Atf("  echo \", \" . json_encode(nError) . \":\" . json_encode($nError);");
      Atf("  echo \", \" . json_encode(szError) . \":\" . json_encode($szError) . \"}\";");
      Atf("");
      Atf("");
      Atf("//__Catch Exception");
      Atf("}");
      Atf("catch(Exception $ex){");
      Atf("  echo \"Error-Exception: \" . $ex->getMessage();");
      Atf("}");
      Atf("");
      Atf("//Step9 - Close connection");
      Atf("$conn = null;");
      Atf("");
      Atf("?>");
      Atf("");

      FileManager.writeNewFileWithContent(filePath, fileName, fileExt, TempFileContent, true);
    }
    private void CreatePHP_loadTable() {
      Console.WriteLine("  -  CreatePHP_loadTable()");
      string filePath = AC.APPath + "\\60_php";
      string fileName = "loadTable";
      string fileExt = "php";
      TempFileContent = new List<string>();

      Atf("<?php");
      Atf("");
      Atf("//Step1 - Init_Variables");
      Atf("header('Content-Type: application/json; charset=UTF-8');");
      Atf("$servername = 'localhost';");
      Atf("$username = 'administrator';");
      Atf("$password = 'uhuAdmin1836';");
      Atf("$dbname = 'dbAktivenplaner';");
      Atf("$query = \"SELECT * from tblPerson\";");
      Atf("");
      Atf("//__Start-Try");
      Atf("try{");
      Atf("//  Step2 - Database_Connection");
      Atf("  $conn = new PDO(\"mysql: host=$servername; dbname=$dbname\", $username, $password);");
      Atf("  $conn or die('Error connecting to MySQL server.');");
      Atf("  $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);");
      Atf("");
      Atf("//  Step3 - Get_Parameters");
      Atf("  $table = $_GET[\"tablename\"];");
      Atf("");
      Atf("//  Step4 - Prepare values");
      Atf("  $query = \"SELECT * FROM \" . $table;");
      Atf("");
      Atf("//  Step5 - Prepare connection");
      Atf("  $stmt = $conn->prepare($query);");
      Atf("  //$stmt = $conn->query($query);");
      Atf("  $stmt or die('Error preparing statement.');");
      Atf("  //$stmt->bindParam(':table', $table, PDO::PARAM_STR, 1000);");
      Atf("");
      Atf("//  Step6 - Execute DB_Connection");
      Atf("  $result = $stmt->execute();");
      Atf("  $result or die('Error executing statement.');");
      Atf("");
      Atf("//  Step7 - Get Results");
      Atf("  //$result = $stmt->fetchAll();");
      Atf("  echo \"[\";");
      Atf("  $thelp = 0;");
      Atf("  while ($row = $stmt->fetchObject()) {");
      Atf("    if($thelp==0){");
      Atf("      echo json_encode($row);");
      Atf("      $thelp++;");
      Atf("    }");
      Atf("    else{");
      Atf("      echo \", \" . json_encode($row);");
      Atf("    }");
      Atf("  }");
      Atf("  echo \"]\";");
      Atf("");
      Atf("//  Step8 - Return result");
      Atf("  /*if($result==null){");
      Atf("    echo \"Error.Result was null.\";");
      Atf("  }");
      Atf("  else{");
      Atf("    echo $result;");
      Atf("    echo json_encode($result);");
      Atf("  }*/");
      Atf("");
      Atf("//__Catch Exception");
      Atf("}");
      Atf("catch(Exception $ex){");
      Atf("  echo \"Error - Exception: \" . $ex->getMessage();");
      Atf("}");
      Atf("");
      Atf("//Step9 - Close connection");
      Atf("$conn = null;");
      Atf("");
      Atf("?>");
      Atf("");

      FileManager.writeNewFileWithContent(filePath, fileName, fileExt, TempFileContent, true);
    }
    #endregion create_php

    #region helper_methods
    private string GetEnumName(string inString) {
      string s = inString;
      s = s.Replace("!", "");
      s = s.Replace(" ", "_");
      return s;
    }
    private void Atf(string inLine) {
      if (TempFileContent == null) {
        throw new Exception("ERROR@Creator_SQL.Atf: TempFileContent was null!");
      }
      if (inLine == null) {
        throw new Exception("ERROR@Creator_SQL.Atf: Parameter inLine was null!");
      }
      TempFileContent.Add(inLine);
    }
    private void Atf_multi(List<string> inLines) {
      if (TempFileContent == null) {
        throw new Exception("ERROR@Creator_SQL.Atf: TempFileContent was null!");
      }
      if (inLines == null) {
        throw new Exception("ERROR@Creator_SQL.Atf: Parameter inLines was null!");
      }
      TempFileContent.AddRange(inLines);
    }
    private string GetPDOType(CSV_Spalte inSpalte) {
      string s = "";
      if (inSpalte.Art == "int") {
        s = "PDO::PARAM_INT";
      }
      else {
        s = "PDO::PARAM_STR,1000";
      }
      return s;
    }
    #endregion helper_methods

  }

}
