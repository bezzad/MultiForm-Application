﻿using System;
using Dapper;
using System.Data;
using System.Windows.Forms;
using BlurMessageBox;
using Newtonsoft.Json;
using System.IO;

namespace FileExporter.PluginForms
{
    public partial class InWay : BaseForm
    {
        private bool _inProcess;

        public InWay()
        {
            InitializeComponent();
        }

        private async void btnShow_Click(object sender, System.EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                lock (SyncObj)
                {
                    if (_inProcess) return;
                    _inProcess = true;
                }

                if (!ValidateInputs()) return;

                var parameters = new { OldInvoiceId = txtInvoiceId.Value, InvoiceTypeID = 4, RunDate = DateTime.Now.GetPersianDateNumber() };
                //
                var source =
                    await
                        Connections.SaleCore.SqlConn.ExecuteReaderAsync("sp_GetOldSaleInvoiceDetails", parameters,
                            commandType: CommandType.StoredProcedure, transaction: null, commandTimeout: 1000);
                SourceTable = new DataTable();
                SourceTable.Load(source);
                SetGridData();

                //
                dgvMain.SetHeaderNames();
            }
            catch (Exception ex)
            {
                MsgBox.Show(ex.Message, ex.Source, Buttons.OK, Icons.Error, AnimateStyle.SlideDown);
            }
            finally
            {
                Cursor = Cursors.Default;
                _inProcess = false;
            }
        }
        private async void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                lock (SyncObj)
                {
                    if (_inProcess) return;
                    _inProcess = true;
                }

                if (!ValidateInputs()) return;

                var data = JsonConvert.SerializeObject(SourceTable, Formatting.Indented);

                var path = ExtensionsFramework.GetSaveFilePath("InWay", txtInvoiceId.Value);

                await data.SaveJsonAsync(path, true);
            }
            catch (Exception ex)
            {
                MsgBox.Show(ex.Message, ex.Source, Buttons.OK, Icons.Error, AnimateStyle.SlideDown);
            }
            finally
            {
                _inProcess = false;
            }
        }
        private async void btnOpenJson_Click(object sender, EventArgs e)
        {
            try
            {
                lock (SyncObj)
                {
                    if (_inProcess) return;
                    _inProcess = true;
                }

                var path = ExtensionsFramework.GetOpenFilePath("InWay");

                var opFile = new FileInfo(path);

                if (opFile.Extension == ".xls")
                {
                    SourceTable = await path.ReadXlsFileAsync();
                }
                else if (opFile.Extension == ".dbi")
                {
                    SourceTable = await path.ReadJsonFileAsync();
                }
                else
                {
                    MsgBox.Show("فایل مورد نظر نا معتبر میباشد!");
                    return;
                }


                SetGridData();

                //
                dgvMain.SetHeaderNames();
            }
            catch (Exception ex)
            {
                MsgBox.Show(ex.Message, ex.Source, Buttons.OK, Icons.Error, AnimateStyle.SlideDown);
            }
            finally
            {
                _inProcess = false;
            }
        }


        private bool ValidateInputs()
        {
            if (!string.IsNullOrEmpty(txtInvoiceId.Value)) return true;

            MsgBox.Show("لطفا شماره فاکتور را وارد فرمائید", "شماره فاکتور خالی", Buttons.OK, Icons.Error, AnimateStyle.SlideDown);
            txtInvoiceId.Focus();

            return false;
        }
        private void SetGridData()
        {
            dgvMain.DataSource = SourceTable;
            var count = SourceTable.Rows.Count;
            gbInWays.Text = $"جزئیات توراهی (تعداد: {count})";
        }
    }
}