$(document).ready(function () {
    if ($(".tblCRFormList").length > 0) {
        $(".tblCRFormList").tablesorter({
            theme: "bootstrap",
            widthFixed: true,

            // widget code contained in the jquery.tablesorter.widgets.js file
            // use the zebra stripe widget if you plan on hiding any rows (filter widget)
            // the uitheme widget is NOT REQUIRED!
            widgets: ["filter", "columns", "stickyHeaders"],

            widgetOptions: {
                //filter_excludeFilter: {
                //    // zero-based column index
                //    7: 'range',
                //    8: 'range',
                //    10: 'range'
                //},
                // class names added to columns when sorted
                columns: ["primary", "secondary", "tertiary"],

                // extra css class name (string or array) added to the filter element (input or select)
                filter_cssFilter: [
                    'form-control',
                    'form-control',
                    'form-control',
                    'form-control',
                    'form-control',
                    'form-control',
                    'form-control',
                    'form-control'
                ], filter_defaultFilter: {
                    // "{query} - a single or double quote signals an exact filter search
                    6: '"{q}'
                }
            }
        }).tablesorterPager({
            cssGoto: '.pagenum',
            container: $(".ts-pager"),
            output: '{startRow} to {endRow} ({totalRows})',
            size: 'all'
        });
    }
    var hotA, hotB, hotC;
    var hotData = [];
    if ($(".divCompetencyResult").length > 0) {
        $(".divCompetencyResult").each(function () {
            generateCompetency($(this));
        });
        const _readOnlyHeader = ["A. Pengetahuan Teknis / Technical Knowledge", "B. Kemampuan Praktik / Practical Skill", "C. Perilaku / Behaviour"]
        const _initMerge = [];
        function generateCompetency(currDiv) {
            const currID = currDiv.attr("id");
            const currCompetency = currDiv.data("competency");
            const container = document.querySelector('#' + currID);
            $.ajax({
                type: "POST",
                url: "/NGKBusi/HC/Competency/getResultData",
                tryCount: 0,
                tryLimit: 3,
                data: {
                    iNIK: $(".lblNIK").data("nik"),
                    iName: $(".lblName").data("name"),
                    iDivision: $(".lblDivision").data("division"),
                    iDepartment: $(".lblDepartment").data("department"),
                    iSection: $(".lblSection").data("section"),
                    iCostName: $(".lblCostName").data("costname"),
                    iPosition: $(".lblPosition").data("position"),
                    iTitleName: $(".lblTitleName").data("titlename"),
                    iCompetency: currCompetency,
                },
                success: function (data) {
                    //console.log(data);
                    generateHansontable(data, container, currCompetency);
                }, error: function (xhr, textStatus, errorThrown) {
                    if (textStatus === "timeout") {
                        this.tryCount++;
                        if (this.tryCount <= this.tryLimit) {
                            $.ajax(this);
                            return;
                        }
                    }
                    alert("Error Occurred, Please try again !");
                }
            });
        }
        function generateHansontable(currData, container, competency) {
            const hot = new Handsontable(container, {
                nestedHeaders: [
                    ['', '', '', { label: 'HASIL PENILAIAN', colspan: 7 }, ''],
                    ['NO', 'PERSYARATAN KOMPETENSI', 'Std Nilai', '5', '4', '3', '2', '1', 'NILAI', 'HASIL', 'CATATAN']
                ],
                formulas: {
                    engine: HyperFormula,
                },
                data: currData,
                columns: [{ width: '35px', readOnly: true }, { readOnly: true }, { type: 'numeric', width: '35px', className: 'htCenter', readOnly: true, numericFormat: { pattern: '0,00', culture: 'en-US' } }
                    , { type: 'checkbox', width: '35px', className: 'htCenter' }, { type: 'checkbox', width: '35px', className: 'htCenter' }, { type: 'checkbox', width: '35px', className: 'htCenter' }, { type: 'checkbox', width: '35px', className: 'htCenter' }, { type: 'checkbox', width: '35px', className: 'htCenter' }
                    , { type: 'numeric', width: '35px', className: 'htCenter', numericFormat: { pattern: '0,00', culture: 'en-US' } }
                    , {}, {}],
                colHeaders: true,
                contextMenu: {
                    items: {
                        row_above: {
                            hidden() {
                                return this.getSelectedLast() ?.[0] == 0;
                            }
                        },
                        row_below: {},
                        remove_row: {
                            hidden() {
                                return _readOnlyHeader.includes(this.getValue());
                            }
                        },

                    }
                },
                stretchH: 'all',
                height: 'auto',
                width: 'auto',
                mergeCells: true,
                manualColumnResize: true,
                autoWrapRow: true,
                autoWrapCol: true,
                afterLoadData: (sourceData, initialLoad, source) => {
                    sourceData.forEach((v, i) => {
                        if (_readOnlyHeader.includes(v[0])) {
                            _initMerge.push({ row: i, col: 0, rowspan: 1, colspan: 11 })
                        };
                    });
                },
                afterSetDataAtCell: (changes, source) => {
                    alert(changes + "||" + source);
                },
                licenseKey: 'non-commercial-and-evaluation',
            });
            hot.alter('insert_row_below');
            hot.alter('insert_row_below');
            hot.alter('insert_row_below');
            hot.updateSettings({
                cells(row, col) {
                    let upSetting = {}
                    if (_readOnlyHeader.includes(hot.getData()[row][col])) {
                        upSetting.readOnly = true;
                        upSetting.className = "font-weight-bold";
                    };
                    if (row >= this.instance.countRows() - 3) {
                        switch (col) {
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                upSetting.className = "tbFooter htCenter summaryCheckbox";
                                break;
                            case 2:
                            case 8:
                                upSetting.className = "columnSummaryResult tbFooter htCenter";
                                break;
                            default:
                                upSetting.className = "tbFooter htCenter";
                                break;
                        }

                    }
                    return upSetting;
                }, columnSummary: [{
                    sourceColumn: 2,
                    type: 'count',
                    destinationRow: hot.countRows() - 3,
                    destinationColumn: 2,
                    forceNumeric: true
                }, {
                    sourceColumn: 8,
                    type: 'sum',
                    destinationRow: hot.countRows() - 3,
                    destinationColumn: 8,
                    forceNumeric: true
                }, {
                    sourceColumn: 2,
                    type: 'sum',
                    destinationRow: hot.countRows() - 2,
                    destinationColumn: 2,
                    forceNumeric: true
                }, {
                    sourceColumn: 8,
                    type: 'sum',
                    destinationRow: hot.countRows() - 2,
                    destinationColumn: 8,
                    forceNumeric: true
                }]
            });
            hot.setDataAtCell(hot.countRows() - 3, 1, "Summary");
            hot.setDataAtCell(hot.countRows() - 2, 1, "Sub-Total");
            hot.setDataAtCell(hot.countRows() - 1, 1, "Total");
            hot.updateSettings({
                mergeCells: _initMerge
            });
            if (competency == "A") {
                hotA = hot;
                hotData.push({ hotA : hot.getSourceDataArray() });
            } else if (competency == "B") {
                hotB = hot;
                hotData.push({ hotB: hot.getSourceDataArray() });
            } else {
                hotC = hot;
                hotData.push({ hotC: hot.getSourceDataArray() });
            }
            hot.addHook('afterMergeCells', (cellRange, mergeParent) => {
                //const _cellRangeFromRow = cellRange.from.row;
                //const _cellRangeFromCol = cellRange.from.col;
                //const _cellRangeToRow = cellRange.to.row;
                //const _cellRangeToCol = cellRange.to.col;

                //console.log(cellRange);
                //console.log(_cellRangeFromRow);
                //console.log(_cellRangeFromCol);
            });
        }


        $(".btnCRSaveData").click(function () {
            //console.log(hot.validateCells);
            hotA.validateCells((valid) => {
                if (valid) {
                    var _currData = hotData;
                    console.log(_currData);
                    $.ajax({
                        type: "POST",
                        url: "/NGKBusi/HC/Competency/setResultData",
                        tryCount: 0,
                        tryLimit: 3,
                        data: {
                            iNIK: $(".lblNIK").data("nik"),
                            iName: $(".lblName").data("name"),
                            iDivision: $(".lblDivision").data("division"),
                            iDepartment: $(".lblDepartment").data("department"),
                            iSection: $(".lblSection").data("section"),
                            iCostName: $(".lblCostName").data("costname"),
                            iPosition: $(".lblPosition").data("position"),
                            iTitleName: $(".lblTitleName").data("titlename"),
                            iData: _currData
                        },
                        success: function (data) {
                            console.log(data);
                            swal("Success!", "Data has been saved!", "success");
                        }, error: function (xhr, textStatus, errorThrown) {
                            if (textStatus === "timeout") {
                                this.tryCount++;
                                if (this.tryCount <= this.tryLimit) {
                                    $.ajax(this);
                                    return;
                                }
                            }
                            console.log(xhr)
                            console.log(textStatus)
                            console.log(errorThrown)
                            alert("Error Occurred, Please try again !");
                        }
                    });
                } else {
                    swal("Invalid!", "Please check your data format!", "error");
                }
            })
        });
    }
});

