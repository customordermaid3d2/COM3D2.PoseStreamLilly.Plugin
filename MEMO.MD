GameMain.Instance.SysDlg.Show(  "WARNING\n" + 
                                "There are duplicate .menu files in Mod folder. " +
                                "This might cause duplicated items icons in game. Please remove the duplicates.\n\n" + 
                                $"List of all found duplicates was saved to {savePath}", SystemDialog.TYPE.OK);