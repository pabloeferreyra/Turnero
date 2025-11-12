// @ts-nocheck
(function () {
    'use strict';

    const namespace = 'parentsData';
    const tabSelector = '#parentsTab';

    $(document).ready(function () {
        const $tab = $(tabSelector);
        if (!$tab.length) return;

        $tab.off(`click.${namespace}`).on(`click.${namespace}`, function () {
            loadParentsData();
        });

        $('#tabContent')
            .off(`click.${namespace}`, '[data-edit-parent]')
            .on(`click.${namespace}`, '[data-edit-parent]', function () {
                const id = $(this).data('id');
                if (id) {
                    editParent(id);
                }
            });

        $('#tabContent')
            .off(`click.${namespace}`, '[data-detail-parent]')
            .on(`click.${namespace}`, '[data-detail-parent]', function () {
                const id = $(this).data('id');
                detailParent(id);
            });
    });

    function resolvePatientId() {
        return $('#patientId').val() || $('#PatientId').val() || '';
    }

    function loadParentsData() {
        const $tab = $(tabSelector);
        if (!$tab.length) return;

        const patientId = resolvePatientId();
        if (!patientId) return;

        let url = $tab.data('url');
        if (!url) {
            url = `/ParentsData/Index?id=${encodeURIComponent(patientId)}`;
        }

        $('#myTabs .nav-link').removeClass('active');
        $tab.addClass('active');

        $('#tabContent').load(url, function () {
            document.dispatchEvent(new Event('parents:dataLoaded'));
        });
    }

    function detailParent(id) {
        const targetId = id || resolvePatientId();
        if (!targetId) return;

        const url = `/ParentsData/Index?id=${encodeURIComponent(targetId)}`;
        $('#tabContent').load(url, function () {
            document.dispatchEvent(new Event('parents:detailLoaded'));
        });
    }

    function editParent(id) {
        const targetId = id || resolvePatientId();
        if (!targetId) return;

        fetch(`/ParentsData/Edit?id=${encodeURIComponent(targetId)}`)
            .then(r => r.text())
            .then(html => {
                $('#EditFormContent').html(html);
                $('#Edit').modal('show');
                document.dispatchEvent(new Event('parents:editLoaded'));
            });
    }

    window.parents_loadData = loadParentsData;
})();
