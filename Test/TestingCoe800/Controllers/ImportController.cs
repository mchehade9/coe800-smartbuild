﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


using System.Data;
using System.Data.OleDb;
using System.IO;


using System.Data.Entity;

using System.Threading.Tasks;
using System.Net;

using TestingCoe800.Models;


namespace TestingCoe800.Controllers
{
    public class ImportController : Controller
    {
        // GET: Import
        private UsersDBEntities db = new UsersDBEntities();
        [HttpPost]
        public ActionResult Import(HttpPostedFileBase postedFile)
        {

            if (postedFile != null)
            {
                try
                {
                    string fileExtension = Path.GetExtension(postedFile.FileName);

                    //Validate uploaded file and return error.
                    if (fileExtension != ".xls" && fileExtension != ".xlsx")
                    {
                        ViewBag.Message = "Please select the excel file with .xls or .xlsx extension";
                        return View();
                    }
                     
                    string folderPath = Server.MapPath("~/UploadedFiles/");
                    //Check Directory exists else create one
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    //Save file to folder
                    var filePath = folderPath + Path.GetFileName(postedFile.FileName);
                    postedFile.SaveAs(filePath);

                    //Get file extension

                    string excelConString = "";

                    //Get connection string using extension 
                    switch (fileExtension)
                    {
                        //If uploaded file is Excel 1997-2007.
                        case ".xls":
                            excelConString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 8.0;HDR=YES'";
                            break;
                        //If uploaded file is Excel 2007 and above
                        case ".xlsx":
                            excelConString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 8.0;HDR=YES'";
                            break;
                    }

                    //Read data from first sheet of excel into datatable
                    DataTable dt = new DataTable();
                    excelConString = string.Format(excelConString, filePath);

                    using (OleDbConnection excelOledbConnection = new OleDbConnection(excelConString))
                    {
                        using (OleDbCommand excelDbCommand = new OleDbCommand())
                        {
                            using (OleDbDataAdapter excelDataAdapter = new OleDbDataAdapter())
                            {
                                excelDbCommand.Connection = excelOledbConnection;

                                excelOledbConnection.Open();
                                //Get schema from excel sheet
                                DataTable excelSchema = GetSchemaFromExcel(excelOledbConnection);
                                //Get sheet name
                                string sheetName = excelSchema.Rows[0]["TABLE_NAME"].ToString();
                                excelOledbConnection.Close();

                                //Read Data from First Sheet.
                                excelOledbConnection.Open();
                                excelDbCommand.CommandText = "SELECT * From [" + sheetName + "]";
                                excelDataAdapter.SelectCommand = excelDbCommand;
                                //Fill datatable from adapter
                                excelDataAdapter.Fill(dt);
                                excelOledbConnection.Close();
                            }
                        }
                    }

                    //Insert records to Employee table.
                    using (var context = new UsersDBEntities()) ;
                    {
                        //Loop through datatable and add employee data to employee table. 
                        foreach (DataRow row in dt.Rows)
                        {
                            
                            db.StoreItems.Add(GetItem(row));
                        }
                        db.SaveChanges();
                    }
                    ViewBag.Message = "Data Imported Successfully.";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                }
            }
            else
            {
                ViewBag.Message = "Please select the file first to upload.";
            }
            return View();
        }

        private static DataTable GetSchemaFromExcel(OleDbConnection excelOledbConnection)
        {
            return excelOledbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
        }

        //Convert each datarow into employee object
        private StoreItem GetItem(DataRow row)
        {
            return new StoreItem
            {
                Id = int.Parse(row[0].ToString()),
                StoreIDFk = int.Parse(row[1].ToString()),
                ItemName = row[2].ToString(),
                Price = (row[3].ToString())
            };
        }
    }
}