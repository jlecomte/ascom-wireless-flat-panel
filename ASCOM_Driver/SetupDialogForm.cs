/*
 * SetupDialogForm.cs
 * Copyright (C) 2022 - Present, Julien Lecomte - All Rights Reserved
 * Licensed under the MIT License. See the accompanying LICENSE file for terms.
 */

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using System.Diagnostics;
using Windows.Devices.Bluetooth.Advertisement;
using System.Linq;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ASCOM.DarkSkyGeek
{
    // Form not registered for COM!
    [ComVisible(false)]

    public partial class SetupDialogForm : Form
    {
        WirelessFlatPanel wirelessFlatPanel;
        BluetoothLEAdvertisementWatcher watcher;
        ulong bleDeviceAddress;

        public SetupDialogForm(WirelessFlatPanel wirelessFlatPanel)
        {
            InitializeComponent();
            this.wirelessFlatPanel = wirelessFlatPanel;
        }

        private string getFormattedBluetoothAddress(ulong address)
        {
            string hexValue = address.ToString("X");
            if (hexValue.Length == 12)
            {
                return hexValue.Insert(10, ":").Insert(8, ":").Insert(6, ":").Insert(4, ":").Insert(2, ":");
            }
            throw new Exception("Invalid Bluetooth Address: " + address);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            wirelessFlatPanel.tl.Enabled = chkTrace.Checked;
            wirelessFlatPanel.bleDeviceAddress = bleDeviceAddress;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BrowseToHomepage(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/jlecomte/wireless-flat-panel");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void SetupDialogForm_Load(object sender, EventArgs e)
        {
            chkTrace.Checked = wirelessFlatPanel.tl.Enabled;
            bleDeviceAddress = wirelessFlatPanel.bleDeviceAddress;

            // Adjust the position of the label so it looks decent...
            pairedDeviceAddrValue.Location = new Point(pairedDeviceAddrLbl.Location.X + pairedDeviceAddrLbl.Width, pairedDeviceAddrLbl.Location.Y);

            try
            {
                pairedDeviceAddrValue.Text = getFormattedBluetoothAddress(wirelessFlatPanel.bleDeviceAddress);
                pairedDeviceAddrValue.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception)
            {
                pairedDeviceAddrValue.Text = "No device paired";
                pairedDeviceAddrValue.ForeColor = System.Drawing.Color.Red;
            }

            if (wirelessFlatPanel.Connected)
            {
                chkTrace.Enabled = false;
                devicesListBox.Enabled = false;
                deviceSelectionBtn.Enabled = false;
            }
            else
            {
                watcher = new BluetoothLEAdvertisementWatcher
                {
                    ScanningMode = BluetoothLEScanningMode.Active
                };

                watcher.Received += (w, args) =>
                {
                    var uuids = args.Advertisement.ServiceUuids;
                    foreach (var uuid in uuids)
                    {
                        if (uuid.Equals(WirelessFlatPanel.BLE_SERVICE_UUID))
                        {
                            ulong address = args.BluetoothAddress;

                            devicesListBox.Invoke(new Action(() =>
                            {
                                // Was this device previously added to the list? Let's find out...
                                bool found = devicesListBox.Items.Cast<ListBoxItem>().Any(x => x.BluetoothAddress == address);

                                // If not, add it to the list so it can be selected:
                                if (!found)
                                {
                                    ListBoxItem item = new ListBoxItem
                                    {
                                        Text = getFormattedBluetoothAddress(address),
                                        BluetoothAddress = address
                                    };
                                    devicesListBox.Items.Add(item);
                                }
                            }));
                        }
                    }
                };

                watcher.Start();
            }
        }

        private void SetupDialogForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (watcher != null)
            {
                watcher.Stop();
            }
        }

        private void devicesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            deviceSelectionBtn.Enabled = devicesListBox.SelectedIndex != -1;
        }

        private void deviceSelectionBtn_Click(object sender, EventArgs e)
        {
            ListBoxItem item = devicesListBox.SelectedItem as ListBoxItem;
            pairedDeviceAddrValue.Text = item.Text;
            bleDeviceAddress = item.BluetoothAddress;
            pairedDeviceAddrValue.ForeColor = System.Drawing.Color.Green;
        }
    }

    class ListBoxItem
    {
        public string Text { get; set; }
        public ulong BluetoothAddress { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
