﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using fyiReporting.RdlAsp;
using System.Data.SQLite;
using System.Data;

namespace ReportServer
{
 
    public partial class ShowReport : System.Web.UI.Page
    {

        public RdlReport _Report { get; set; }
        
        //GJL 20080520 - Show report parameters without running it first (many line changes in this file)
        public bool error {get; set;}

        public string Name { get; set; }

        public ShowReport()
        {
            _Report = new RdlReport();
        }

        private bool HasPermissions(string reportName)
        {
            string nameOnly = System.IO.Path.GetFileName(reportName);
            string sql = "SELECT a.reportname, a.tag, c.description FROM reportfiles a ";
            sql += " JOIN roleaccess b ON a.tag=b.tag JOIN roletags c ON a.tag = c.tag ";
            sql += " WHERE b.role = @roleid AND a.reportname=@reportname;";
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = new SQLiteConnection(Code.DAL.ConnectionString);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = sql;
            cmd.Parameters.Add("@roleid", DbType.String).Value = SessionVariables.LoggedRoleId;
            cmd.Parameters.Add("@reportname", DbType.String).Value = nameOnly;

            DataTable dt = Code.DAL.ExecuteCmdTable(cmd);

            if (dt.Rows.Count > 0)
            {
                return true;
            }

            return false;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Name = Request.QueryString["rs:Name"];
            string FirstRun = Request.QueryString["rs:FirstRun"];

            if (HasPermissions(Request.QueryString["rs:url"]) == false)
            {
                return;
            }

    

            if (FirstRun == "true")
            {
                _Report.NoShow = true;
            }
            else
            {
                _Report.NoShow = false;
            }
    
	        string arg = Request.QueryString["rs:Format"];
            if (arg != null)
            {
                _Report.RenderType = arg;
            }
            else
            {
                _Report.RenderType = "html";
            }
            _Report.PassPhrase = "northwind";       // user should provide in some fashion (from web.config??)
	        // ReportFile must be the last item set since it triggers the building of the report
	        arg = Request.QueryString["rs:url"];
	        if (arg != null)
		        _Report.ReportFile = arg;
	
	        switch (_Report.RenderType)
	        {
		        case "xml":
		
                    if (_Report.Xml == null)
                    {
                        error = true; 
                    }
                    else
                    {	
			        Response.ContentType = "application/xml";
			        Response.Write(_Report.Xml);
                    }
			        break;
		        case "pdf":
                    if (_Report.Object == null)
                    {
                        error = true;
                    }
                    else
                    { 
			        Response.ContentType = "application/pdf";
			        Response.BinaryWrite(_Report.Object);
                    }			
			        break;
                case "csv":
          
                    if (_Report.CSV == null)
                    {
                        error = true;
                    }
                    else
                    {  
                    Response.ContentType = "text/plain";
                    Response.Write(_Report.CSV);
                    }
                    break;
                case "html":	// Rest of the page takes care of the general html
		        default:
			        break;
	        }
        }

        public string Meta
        {
            get
            {
                if (_Report.ReportFile == "statistics")
                    return "<meta http-equiv=\"Refresh\" contents=\"10\"/>";
                else
                    return "";
            }
        }
    }
}