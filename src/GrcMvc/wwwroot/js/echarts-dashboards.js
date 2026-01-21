// GRC ECharts Dashboard Components
// ============================================================================
// Beautiful, interactive charts for the multi-tenant admin UI
// Uses Apache ECharts for "wow" visuals
// ============================================================================

// Initialize ECharts with tenant data
const GrcCharts = {
    // Color palette matching GRC theme
    colors: {
        primary: '#0d6efd',
        success: '#198754',
        warning: '#ffc107',
        danger: '#dc3545',
        info: '#0dcaf0',
        dark: '#212529',
        gradient: ['#667eea', '#764ba2']
    },

    // Compliance Score Gauge
    complianceGauge: function(containerId, score) {
        const chart = echarts.init(document.getElementById(containerId));
        const option = {
            series: [{
                type: 'gauge',
                startAngle: 180,
                endAngle: 0,
                min: 0,
                max: 100,
                splitNumber: 10,
                itemStyle: {
                    color: score >= 80 ? this.colors.success :
                           score >= 50 ? this.colors.warning : this.colors.danger
                },
                progress: {
                    show: true,
                    roundCap: true,
                    width: 18
                },
                pointer: { show: false },
                axisLine: {
                    roundCap: true,
                    lineStyle: { width: 18, color: [[1, '#e9ecef']] }
                },
                axisTick: { show: false },
                splitLine: { show: false },
                axisLabel: { show: false },
                title: {
                    offsetCenter: [0, '30%'],
                    fontSize: 14,
                    color: '#6c757d'
                },
                detail: {
                    offsetCenter: [0, '-10%'],
                    valueAnimation: true,
                    formatter: '{value}%',
                    fontSize: 32,
                    fontWeight: 'bold',
                    color: 'inherit'
                },
                data: [{ value: score, name: 'Compliance Score' }]
            }]
        };
        chart.setOption(option);
        return chart;
    },

    // Risk Heatmap (5x5 matrix)
    riskHeatmap: function(containerId, data) {
        const chart = echarts.init(document.getElementById(containerId));
        const likelihood = ['Rare', 'Unlikely', 'Possible', 'Likely', 'Almost Certain'];
        const impact = ['Negligible', 'Minor', 'Moderate', 'Major', 'Catastrophic'];

        // Transform data to heatmap format
        const heatmapData = [];
        for (let i = 0; i < 5; i++) {
            for (let j = 0; j < 5; j++) {
                const count = data[i] && data[i][j] ? data[i][j] : 0;
                heatmapData.push([j, i, count]);
            }
        }

        const option = {
            tooltip: {
                position: 'top',
                formatter: function(params) {
                    return `${likelihood[params.data[1]]} × ${impact[params.data[0]]}<br/>` +
                           `<strong>${params.data[2]} risks</strong>`;
                }
            },
            grid: { left: '15%', right: '10%', top: '10%', bottom: '15%' },
            xAxis: {
                type: 'category',
                data: impact,
                splitArea: { show: true },
                axisLabel: { rotate: 45, fontSize: 10 }
            },
            yAxis: {
                type: 'category',
                data: likelihood,
                splitArea: { show: true },
                axisLabel: { fontSize: 10 }
            },
            visualMap: {
                min: 0,
                max: 10,
                calculable: true,
                orient: 'horizontal',
                left: 'center',
                bottom: '0%',
                inRange: {
                    color: ['#e8f5e9', '#fff9c4', '#ffcc80', '#ef9a9a', '#b71c1c']
                }
            },
            series: [{
                name: 'Risk Count',
                type: 'heatmap',
                data: heatmapData,
                label: {
                    show: true,
                    formatter: function(params) {
                        return params.data[2] > 0 ? params.data[2] : '';
                    }
                },
                emphasis: {
                    itemStyle: { shadowBlur: 10, shadowColor: 'rgba(0, 0, 0, 0.5)' }
                }
            }]
        };
        chart.setOption(option);
        return chart;
    },

    // Compliance Trend Line Chart
    complianceTrend: function(containerId, dates, scores) {
        const chart = echarts.init(document.getElementById(containerId));
        const option = {
            tooltip: {
                trigger: 'axis',
                formatter: '{b}<br/>Score: <strong>{c}%</strong>'
            },
            grid: { left: '10%', right: '5%', top: '10%', bottom: '15%' },
            xAxis: {
                type: 'category',
                data: dates,
                axisLabel: { rotate: 45, fontSize: 10 }
            },
            yAxis: {
                type: 'value',
                min: 0,
                max: 100,
                axisLabel: { formatter: '{value}%' }
            },
            series: [{
                data: scores,
                type: 'line',
                smooth: true,
                symbol: 'circle',
                symbolSize: 8,
                lineStyle: { width: 3, color: this.colors.primary },
                areaStyle: {
                    color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                        { offset: 0, color: 'rgba(13, 110, 253, 0.4)' },
                        { offset: 1, color: 'rgba(13, 110, 253, 0.05)' }
                    ])
                },
                markLine: {
                    silent: true,
                    data: [{ yAxis: 80, lineStyle: { color: this.colors.success } }]
                }
            }]
        };
        chart.setOption(option);
        return chart;
    },

    // Tenant Usage Pie Chart
    tenantUsagePie: function(containerId, data) {
        const chart = echarts.init(document.getElementById(containerId));
        const option = {
            tooltip: {
                trigger: 'item',
                formatter: '{b}: {c} ({d}%)'
            },
            legend: {
                orient: 'vertical',
                right: '5%',
                top: 'center'
            },
            series: [{
                type: 'pie',
                radius: ['40%', '70%'],
                center: ['40%', '50%'],
                avoidLabelOverlap: false,
                itemStyle: {
                    borderRadius: 10,
                    borderColor: '#fff',
                    borderWidth: 2
                },
                label: { show: false },
                emphasis: {
                    label: { show: true, fontSize: 14, fontWeight: 'bold' }
                },
                labelLine: { show: false },
                data: data
            }]
        };
        chart.setOption(option);
        return chart;
    },

    // Tasks by Status Bar Chart
    tasksByStatus: function(containerId, categories, pending, completed, overdue) {
        const chart = echarts.init(document.getElementById(containerId));
        const option = {
            tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
            legend: { data: ['Pending', 'Completed', 'Overdue'], bottom: '0%' },
            grid: { left: '10%', right: '5%', top: '10%', bottom: '20%' },
            xAxis: { type: 'category', data: categories },
            yAxis: { type: 'value' },
            series: [
                {
                    name: 'Pending',
                    type: 'bar',
                    stack: 'total',
                    emphasis: { focus: 'series' },
                    data: pending,
                    itemStyle: { color: this.colors.warning }
                },
                {
                    name: 'Completed',
                    type: 'bar',
                    stack: 'total',
                    emphasis: { focus: 'series' },
                    data: completed,
                    itemStyle: { color: this.colors.success }
                },
                {
                    name: 'Overdue',
                    type: 'bar',
                    stack: 'total',
                    emphasis: { focus: 'series' },
                    data: overdue,
                    itemStyle: { color: this.colors.danger }
                }
            ]
        };
        chart.setOption(option);
        return chart;
    },

    // Framework Comparison Radar Chart
    frameworkComparison: function(containerId, frameworks, scores) {
        const chart = echarts.init(document.getElementById(containerId));
        const option = {
            tooltip: {},
            radar: {
                indicator: frameworks.map(f => ({ name: f, max: 100 })),
                shape: 'polygon',
                splitNumber: 5,
                axisName: { color: '#6c757d' },
                splitLine: { lineStyle: { color: '#dee2e6' } },
                splitArea: {
                    areaStyle: { color: ['#f8f9fa', '#e9ecef', '#dee2e6', '#ced4da', '#adb5bd'].reverse() }
                }
            },
            series: [{
                type: 'radar',
                data: [{
                    value: scores,
                    name: 'Compliance',
                    areaStyle: { color: 'rgba(13, 110, 253, 0.3)' },
                    lineStyle: { color: this.colors.primary, width: 2 }
                }]
            }]
        };
        chart.setOption(option);
        return chart;
    },

    // Activity Timeline
    activityTimeline: function(containerId, events) {
        const chart = echarts.init(document.getElementById(containerId));
        const dates = events.map(e => e.date);
        const counts = events.map(e => e.count);

        const option = {
            tooltip: {
                trigger: 'axis',
                formatter: '{b}<br/>Events: <strong>{c}</strong>'
            },
            grid: { left: '10%', right: '5%', top: '10%', bottom: '15%' },
            xAxis: {
                type: 'category',
                data: dates,
                axisLabel: { fontSize: 10 }
            },
            yAxis: { type: 'value' },
            series: [{
                data: counts,
                type: 'bar',
                itemStyle: {
                    color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                        { offset: 0, color: this.colors.primary },
                        { offset: 1, color: '#764ba2' }
                    ])
                },
                emphasis: { itemStyle: { color: this.colors.info } }
            }]
        };
        chart.setOption(option);
        return chart;
    },

    // Resize all charts on window resize
    resizeAll: function() {
        const charts = document.querySelectorAll('[id^="chart-"]');
        charts.forEach(el => {
            const chart = echarts.getInstanceByDom(el);
            if (chart) chart.resize();
        });
    }
};

// Auto-resize on window resize
window.addEventListener('resize', () => GrcCharts.resizeAll());
