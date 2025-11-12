// @ts-nocheck
(() => {
    'use strict';
    const key = "parents";
    const $tab = $('#parentsTab');
    
    $(document).ready(function(){
        AppUtils.Sort.attachHeaderSorting("#Parents", key, loadData);
        window._reloadData = loadData;

        loadData();

        $tab.off('click.parents').on('click.parents', function() {
            $('#myTabs .nav-link').removeClass('active');
            $(this).addClass('active');

            const url = $(this).data('url');
            if (!url) return;

            if (!$('#parents').length) {
                $('#tabContent').load(url, function () {
                    loadData();
                });
            } else {
                loadData();
            }
        });
    })

    $(document).ready(function () {
        $(document).on("click", "[data-edit-parent]", function () {
            EditParent($(this).data("id"));
        });
    });

    $(document).ready(function () {
        $(document).on("click", "[data-detail-parent]", function () {
            console.log($(this).data("id"));
            DetailParent($(this).data("id"));
        });
    });

    function DetailParent(id) {
        fetch(`/Parents/Details/${id}`)
        .then(r => r.text())
        .then(html => {
            $("#DetailFormContent").html(html);
            $("#Detail").modal("show");
            document.dispatchEvent(new Event("parents:detailLoaded"));
        });
    }


    function EditParent(id) {
        fetch(`/Parents/Edit/${id}`)
            .then(r => r.text())
            .then(html => {
                $("#CreateFormContent").html(html);
                $("#Create").modal("show");
                
                document.dispatchEvent(new Event("parents:editLoaded"));
                window.parents_loadData = loadData;    
            });
    }
});