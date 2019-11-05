import React from 'react';
import { Link } from 'react-router-dom';
import styled from 'styled-components';

const NavWrapperStyle = styled.ul`
    position: fixed;
    width: 100%;
    top: 0;
    margin: 0;
    padding: 0;
    overflow: hidden;
    list-style-type: none;
    background-color: #3d3d3d;
`;

const NavItemWrapperStyle = styled.div`
    position: absolute;
    min-width: 100%;
    z-index: -1;
`;

const NavItemStyle = styled.li`
    float: left;

    a {
        display: block;
        font-family: 'Oxygen', sans-serif;
        letter-spacing: 4px;
        font-size: 14px;
        color: white;
        padding: 30px 18px;
        text-decoration: none;
        transition: all 300ms ease;

        &:hover {
            color: #3498db;
        }
    }
`;

const NavigationBar: React.FC = () => {
    return (
        <NavWrapperStyle>
            <NavItemStyle>
                <Link to="/">[LOGO] MINDA</Link>
            </NavItemStyle>

            <NavItemWrapperStyle className="flex justify-center">
                <NavItemStyle>
                    <Link to="/about">ABOUT</Link>
                </NavItemStyle>
                <NavItemStyle>
                    <Link to="/stat">STAT</Link>
                </NavItemStyle>
                <NavItemStyle>
                    <Link to="/">RANK</Link>
                </NavItemStyle>
            </NavItemWrapperStyle>
        </NavWrapperStyle>
    );
};

export default NavigationBar;
