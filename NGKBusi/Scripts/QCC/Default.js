$(document).ready(function () {
    if ($("#tbQCCDataList").length) {
        $("#tbQCCDataList").tablesorter({
            theme: 'metro-dark',
            widthFixed: true,
            widgets: ['zebra', 'columns', 'filter', 'stickyHeaders', 'output'],
            widgetOptions: {
                output_delivery: 'download',
                output_saveFileName: 'QCCUserList.csv',
                output_replaceCR: '&amp;'
            }
        }).tablesorterPager({
            container: $(".pager"),
            output: '{startRow} to {endRow} ({totalRows})',
            size: 50
        });
    }
    $(".selAnggota").change(function () {
        var sectionArray = [];
        $(".selAnggota").each(function (e) {
            var selected = $(this).find('option:selected', this);
            selected.each(function () {
                var currVal = $(this).val();
                if (currVal !== "") {
                    var section = $(this).data("subsection") ? $(this).data("subsection") : $(this).data("sect");
                    if (!section) {
                        section = $(this).data("dept") ? $(this).data("dept") : $(this).data("div");
                    }
                    sectionArray.push(section);
                }
            });
        });

        $("#hfSection").val($.unique(sectionArray.sort()).join(","));
    });
    $('.tablesorter').on("click", ".btnDownload", function () {
        $("#tbQCCDataList").trigger('outputTable');
    });
    $('.MDDatepicker').datepicker({
        changeMonth: true,
        changeYear: true,
        showButtonPanel: true,
        dateFormat: 'M-yy'
    }).focus(function () {
        var thisCalendar = $(this);
        $('.ui-datepicker-calendar').detach();
        $('.ui-datepicker-close').click(function () {
            var month = $("#ui-datepicker-div .ui-datepicker-month :selected").val();
            var year = $("#ui-datepicker-div .ui-datepicker-year :selected").val();
            thisCalendar.datepicker('setDate', new Date(year, month, 1));
        });
    });
    $(".btnDelete").click(function () {
        var currTR = $(this).closest("tr");
        var currID = $(this).data("id");
        if (confirm("Are you sure want to delete this data ?")) {
            currTR.find("td").css("background-color", "orange");
            $.ajax({
                type: "POST",
                url: "/NGKBusi/QCC/Data/deleteList",
                data: { iID: currID },
                success: function (data) {
                    currTR.remove();
                    $("#tbQCCDataList").trigger("update");
                }, error: function () {
                    $("#tbQCCDataList").trigger("update");
                    alert("Error Occurred, Please try again !");
                }
            });
        }
    });
    $(".btnEdit").click(function () {
        var currTR = $(this).closest("tr");
        var currID = $(this).data("id");
        var currMember = currTR.find("td:eq(6)").data("member").split(",");
        $("#hfQCCID").val(currID);
        $("#QCCForm").attr("action", "/NGKBusi/QCC/Data/editList");
        $("#selPeriod").val(currTR.find("td:eq(0)").text());
        $("#txtGroupName").val(currTR.find("td:eq(1)").text());
        $("#selType").val(currTR.find("td:eq(2)").text()).select2();
        $("#txtTema").val(currTR.find("td:eq(3)").text());
        $("#selFasilitator").val(currTR.find("td:eq(4)").data("nik")).select2();
        $("#selLeader").val(currTR.find("td:eq(5)").data("nik")).select2();
        $("#selAnggota").val(currMember).select2();
        $("#srModal").modal();
    });
    $("#imgQCCAdd").click(function () {
        $("#QCCForm").attr("action", "/NGKBusi/QCC/Data/insertList");
    });
});