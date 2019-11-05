import React from 'react';
import styled from 'styled-components';
import MainLayout from '../layouts/MainLayout';
import MainBackground from '../components/MainBackground';
import SummaryText from '../atoms/MainPage/SummaryText';

const WrapperStyle = styled.div`
    margin-top: 60px;
`;

const Main: React.FC = () => {
    return (
        <MainLayout>
            <MainBackground />
            <WrapperStyle className="container mx-auto">
                <SummaryText />
            </WrapperStyle>
        </MainLayout>
    );
};

export default Main;
