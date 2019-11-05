import React from 'react';
import { Link } from 'react-router-dom';
import styled from 'styled-components';

const NavWrapperStyle = styled.ul`
    list-style-type: none;
    margin: 0;
    padding: 0;
    overflow: hidden;
    background-color: rgba(255, 255, 255, 0);
`;

const NavItemStyle = styled.li`
    float: left;

    a {
        display: block;
        color: #000000;
        text-align: center;
        padding: 28px 18px;
        text-decoration: none;

        &:hover {
            background-color: #eeeeee;
        }
    }
`;

const NavigationBar: React.FC = () => {
    return (
        <NavWrapperStyle>
            <NavItemStyle>
                <Link to="/">MINDA</Link>
            </NavItemStyle>
            <NavItemStyle>
                <Link to="/stat">STATS</Link>
            </NavItemStyle>
            <NavItemStyle>
                <Link to="/">RANK</Link>
            </NavItemStyle>
        </NavWrapperStyle>
    );
};

export default NavigationBar;
